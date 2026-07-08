using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Projects.Commands;

public record DeleteProjectCommand(string ProjectId, string UserId) : IRequest<Result>;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result>
{
    private readonly IAppDbContext       _context;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteProjectCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _context     = context;
        _unitOfWork  = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            return Result.Failure("Invalid project ID.");

        var role = _currentUser.Role;
        if (role != "Owner" && role != "Admin")
            return Result.Forbidden("Only owners and admins can delete projects.");

        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        if (project == null)
            return Result.NotFound("Project not found.");

        _context.Projects.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}