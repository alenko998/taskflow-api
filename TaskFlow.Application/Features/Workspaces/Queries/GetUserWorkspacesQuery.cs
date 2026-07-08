using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Workspaces.Queries;

public record GetUserWorkspacesQuery(string UserId) : IRequest<Result<List<UserWorkspaceResponse>>>;

public record UserWorkspaceResponse(
    Guid          Id,
    string        Name,
    string        Slug,
    WorkspaceRole Role
);

public class GetUserWorkspacesQueryHandler : IRequestHandler<GetUserWorkspacesQuery, Result<List<UserWorkspaceResponse>>>
{
    private readonly IAppDbContext _context;

    public GetUserWorkspacesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserWorkspaceResponse>>> Handle(GetUserWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var workspaces = await _context.WorkspaceMembers
            .Include(m => m.Workspace)
            .Where(m => m.UserId == request.UserId)
            .Select(m => new UserWorkspaceResponse(
                m.Workspace.Id,
                m.Workspace.Name,
                m.Workspace.Slug,
                m.Role
            ))
            .ToListAsync(cancellationToken);

        return Result<List<UserWorkspaceResponse>>.Success(workspaces);
    }
}