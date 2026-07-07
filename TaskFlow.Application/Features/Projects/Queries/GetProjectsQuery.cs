using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Projects.Queries;

public record GetProjectsQuery(string WorkspaceId, string UserId) : IRequest<Result<List<ProjectResponse>>>;

public record ProjectResponse(
    Guid          Id,
    string        Name,
    string        Description,
    ProjectStatus Status,
    TaskPriority  Priority,
    DateTime?     Deadline,
    int           TaskCount,
    int           CompletedTaskCount,
    int           MemberCount,
    DateTime      CreatedAt
);

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, Result<List<ProjectResponse>>>
{
    private readonly IAppDbContext _context;

    public GetProjectsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProjectResponse>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result<List<ProjectResponse>>.Failure("Invalid workspace ID.");

        var projects = await _context.Projects
            .Include(p => p.Tasks)
            .Include(p => p.Members)
            .Where(p => p.WorkspaceId == workspaceId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Status,
                p.Priority,
                p.Deadline,
                p.Tasks.Count,
                p.Tasks.Count(t => t.Status == TaskFlow.Domain.Enums.TaskStatus.Done),
                p.Members.Count,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<List<ProjectResponse>>.Success(projects);
    }
}