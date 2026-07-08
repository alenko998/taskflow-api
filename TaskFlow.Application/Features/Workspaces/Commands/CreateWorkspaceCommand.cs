using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Workspaces.Commands;

public record CreateWorkspaceCommand(
    string Name,
    string UserId = ""
) : IRequest<Result<Guid>>;

public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public CreateWorkspaceCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Guid>.Failure("Workspace name is required.");

        var workspace = Workspace.Create(request.Name, request.UserId);
        _context.Workspaces.Add(workspace);

        var member = WorkspaceMember.Create(workspace.Id, request.UserId, WorkspaceRole.Owner);
        _context.WorkspaceMembers.Add(member);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Created(workspace.Id);
    }
}