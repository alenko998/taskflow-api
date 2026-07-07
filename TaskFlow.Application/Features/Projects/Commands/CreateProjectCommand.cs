using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Projects.Commands;

public record CreateProjectCommand(
    string       Name,
    string       Description,
    TaskPriority Priority,
    DateTime?    Deadline,
    string       WorkspaceId = "",
    string       CreatedById = ""
) : IRequest<Result<Guid>>;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public CreateProjectCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result<Guid>.Failure("Invalid workspace ID.");

        var workspace = await _context.Workspaces.FindAsync(new object[] { workspaceId }, cancellationToken);
        if (workspace == null)
            return Result<Guid>.NotFound("Workspace not found.");

        var project = Project.Create(
            request.Name,
            request.Description,
            request.Priority,
            request.Deadline,
            workspaceId,
            request.CreatedById
        );

        _context.Projects.Add(project);

        var member = ProjectMember.Create(project.Id, request.CreatedById);
        _context.ProjectMembers.Add(member);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Created(project.Id);
    }
}