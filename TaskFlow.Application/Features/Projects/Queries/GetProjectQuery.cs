using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Projects.Queries;

public record GetProjectQuery(string ProjectId, string UserId) : IRequest<Result<ProjectDetailResponse>>;

public record ProjectMemberDto(string UserId, string FirstName, string LastName, string Email);

public record ProjectDetailResponse(
    Guid                   Id,
    string                 Name,
    string                 Description,
    ProjectStatus          Status,
    TaskPriority           Priority,
    DateTime?              Deadline,
    int                    TaskCount,
    int                    CompletedTaskCount,
    List<ProjectMemberDto> Members,
    DateTime               CreatedAt
);

public class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, Result<ProjectDetailResponse>>
{
    private readonly IAppDbContext _context;

    public GetProjectQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProjectDetailResponse>> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            return Result<ProjectDetailResponse>.Failure("Invalid project ID.");

        var project = await _context.Projects
            .Include(p => p.Tasks)
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
            return Result<ProjectDetailResponse>.NotFound("Project not found.");

        return Result<ProjectDetailResponse>.Success(new ProjectDetailResponse(
            Id:                 project.Id,
            Name:               project.Name,
            Description:        project.Description,
            Status:             project.Status,
            Priority:           project.Priority,
            Deadline:           project.Deadline,
            TaskCount:          project.Tasks.Count,
            CompletedTaskCount: project.Tasks.Count(t => t.Status == TaskFlow.Domain.Enums.TaskStatus.Done),
            Members:            project.Members.Select(m => new ProjectMemberDto(m.UserId, m.User.FirstName, m.User.LastName, m.User.Email!)).ToList(),
            CreatedAt:          project.CreatedAt
        ));
    }
}