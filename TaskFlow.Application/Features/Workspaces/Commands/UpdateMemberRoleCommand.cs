using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Workspaces.Commands;

public record UpdateMemberRoleCommand(string WorkspaceId, string TargetUserId, WorkspaceRole NewRole, string RequestedById) : IRequest<Result>;

public class UpdateMemberRoleCommandHandler : IRequestHandler<UpdateMemberRoleCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public UpdateMemberRoleCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result.Failure("Invalid workspace ID.");

        var workspace = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId, cancellationToken);
        if (workspace == null)
            return Result.NotFound("Workspace not found.");

        if (workspace.OwnerId != request.RequestedById)
            return Result.Forbidden("Only the owner can change member roles.");

        if (request.TargetUserId == workspace.OwnerId)
            return Result.Forbidden("Cannot change the owner's role.");

        var member = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == request.TargetUserId, cancellationToken);

        if (member == null)
            return Result.NotFound("Member not found.");

        member.UpdateRole(request.NewRole);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}