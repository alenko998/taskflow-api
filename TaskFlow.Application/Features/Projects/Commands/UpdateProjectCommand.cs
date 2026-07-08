using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Projects.Commands;

public record UpdateProjectCommand(
    string        ProjectId,
    string        Name,
    string        Description,
    TaskPriority  Priority,
    DateTime?     Deadline,
    ProjectStatus Status,
    string        UserId
) : IRequest<Result>;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result>
{
    private readonly IAppDbContext       _context;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateProjectCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _context     = context;
        _unitOfWork  = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            return Result.Failure("Invalid project ID.");

        var role = _currentUser.Role;
        if (role != "Owner" && role != "Admin")
            return Result.Forbidden("Only owners and admins can update projects.");

        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        if (project == null)
            return Result.NotFound("Project not found.");

        project.Update(request.Name, request.Description, request.Priority, request.Deadline, request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}