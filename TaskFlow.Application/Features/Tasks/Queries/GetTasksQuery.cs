using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.Queries;

public record GetTasksQuery(string ProjectId, string UserId) : IRequest<Result<List<TaskResponse>>>;

public record TaskResponse(
    Guid         Id,
    string       Title,
    string       Description,
    TaskStatus   Status,
    TaskPriority Priority,
    DateTime?    DueDate,
    string?      AssigneeId,
    string?      AssigneeName,
    string       CreatedById,
    string       CreatedByName,
    int          CommentCount,
    DateTime     CreatedAt
);

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, Result<List<TaskResponse>>>
{
    private readonly IAppDbContext _context;

    public GetTasksQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TaskResponse>>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            return Result<List<TaskResponse>>.Failure("Invalid project ID.");

        var tasks = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments)
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new TaskResponse(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.AssigneeId,
                t.Assignee != null ? t.Assignee.FullName : null,
                t.CreatedById,
                t.CreatedBy.FullName,
                t.Comments.Count,
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<List<TaskResponse>>.Success(tasks);
    }
}