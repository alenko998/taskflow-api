using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.Commands;

public record AddProjectMemberCommand(string ProjectId, string UserId, string RequestedById) : IRequest<Result>;

public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public AddProjectMemberCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            return Result.Failure("Invalid project ID.");

        var project = await _context.Projects.FindAsync(new object[] { projectId }, cancellationToken);
        if (project == null)
            return Result.NotFound("Project not found.");

        var alreadyMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == request.UserId, cancellationToken);

        if (alreadyMember)
            return Result.Conflict("User is already a member of this project.");

        var member = ProjectMember.Create(projectId, request.UserId);
        _context.ProjectMembers.Add(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}