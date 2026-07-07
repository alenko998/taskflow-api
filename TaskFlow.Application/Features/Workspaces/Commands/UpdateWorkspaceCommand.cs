using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Workspaces.Commands;

public record UpdateWorkspaceCommand(string WorkspaceId, string Name, string UserId) : IRequest<Result>;

public class UpdateWorkspaceCommandHandler : IRequestHandler<UpdateWorkspaceCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public UpdateWorkspaceCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result.Failure("Invalid workspace ID.");

        var workspace = await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == workspaceId, cancellationToken);
        if (workspace == null)
            return Result.NotFound("Workspace not found.");

        if (workspace.OwnerId != request.UserId)
            return Result.Forbidden("Only the owner can update the workspace.");

        workspace.UpdateName(request.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}