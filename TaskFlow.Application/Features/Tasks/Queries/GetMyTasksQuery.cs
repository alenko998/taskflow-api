using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.Queries;

public record GetMyTasksQuery(string UserId, string WorkspaceId) : IRequest<Result<List<TaskResponse>>>;

public class GetMyTasksQueryHandler : IRequestHandler<GetMyTasksQuery, Result<List<TaskResponse>>>
{
    private readonly IAppDbContext _context;

    public GetMyTasksQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TaskResponse>>> Handle(GetMyTasksQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.WorkspaceId, out var workspaceId))
            return Result<List<TaskResponse>>.Failure("Invalid workspace ID.");

        var tasks = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments)
            .Include(t => t.Project)
            .Where(t => t.AssigneeId == request.UserId &&
                        t.Project.WorkspaceId == workspaceId &&
                        t.Status != TaskStatus.Done)
            .OrderBy(t => t.DueDate)
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