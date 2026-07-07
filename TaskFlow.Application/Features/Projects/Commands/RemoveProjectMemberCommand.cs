using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Projects.Commands;

public record RemoveProjectMemberCommand(string ProjectId, string TargetUserId, string RequestedById) : IRequest<Result>;

public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IUnitOfWork   _unitOfWork;

    public RemoveProjectMemberCommandHandler(IAppDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveProjectMemberCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            return Result.Failure("Invalid project ID.");

        var project = await _context.Projects.FindAsync(new object[] { projectId }, cancellationToken);
        if (project == null)
            return Result.NotFound("Project not found.");

        if (project.CreatedById != request.RequestedById)
            return Result.Forbidden("Only the project creator can remove members.");

        if (request.TargetUserId == project.CreatedById)
            return Result.Forbidden("Cannot remove the project creator.");

        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == request.TargetUserId, cancellationToken);

        if (member == null)
            return Result.NotFound("Member not found.");

        _context.ProjectMembers.Remove(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}