using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Workspaces.Commands;

public record RemoveMemberCommand(string WorkspaceId, string TargetUserId, string RequestedById) : IRequest<Result>;

public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public RemoveMemberCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result.Failure("Invalid workspace ID.");

        var workspace = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId, cancellationToken);
        if (workspace == null)
            return Result.NotFound("Workspace not found.");

        if (workspace.OwnerId == request.TargetUserId)
            return Result.Forbidden("Cannot remove the owner from the workspace.");

        var requester = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == request.RequestedById, cancellationToken);

        if (requester == null || (requester.Role != Domain.Enums.WorkspaceRole.Admin && requester.Role != Domain.Enums.WorkspaceRole.Owner))
            return Result.Forbidden("Only admins or owners can remove members.");

        var member = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == request.TargetUserId, cancellationToken);

        if (member == null)
            return Result.NotFound("Member not found.");

        _context.WorkspaceMembers.Remove(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}