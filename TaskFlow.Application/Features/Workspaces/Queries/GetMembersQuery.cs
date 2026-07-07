using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Workspaces.Queries;

public record GetMembersQuery(string WorkspaceId) : IRequest<Result<List<MemberResponse>>>;

public record MemberResponse(
    string       UserId,
    string       FirstName,
    string       LastName,
    string       Email,
    WorkspaceRole Role,
    DateTime     JoinedAt
);

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, Result<List<MemberResponse>>>
{
    private readonly IAppDbContext _context;

    public GetMembersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MemberResponse>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result<List<MemberResponse>>.Failure("Invalid workspace ID.");

        var members = await _context.WorkspaceMembers
            .Include(m => m.User)
            .Where(m => m.WorkspaceId == workspaceId)
            .OrderBy(m => m.JoinedAt)
            .Select(m => new MemberResponse(
                m.UserId,
                m.User.FirstName,
                m.User.LastName,
                m.User.Email!,
                m.Role,
                m.JoinedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<List<MemberResponse>>.Success(members);
    }
}