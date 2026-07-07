using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Application.Features.Tasks.Queries;

public record GetTaskQuery(string TaskId, string UserId) : IRequest<Result<TaskDetailResponse>>;

public record CommentDto(
    Guid     Id,
    string   Content,
    string   AuthorId,
    string   AuthorName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record TaskDetailResponse(
    Guid            Id,
    string          Title,
    string          Description,
    TaskStatus      Status,
    TaskPriority    Priority,
    DateTime?       DueDate,
    string?         AssigneeId,
    string?         AssigneeName,
    string          CreatedById,
    string          CreatedByName,
    Guid            ProjectId,
    List<CommentDto> Comments,
    DateTime        CreatedAt,
    DateTime?       UpdatedAt
);

public class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, Result<TaskDetailResponse>>
{
    private readonly IAppDbContext _context;

    public GetTaskQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TaskDetailResponse>> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TaskId, out var taskId))
            return Result<TaskDetailResponse>.Failure("Invalid task ID.");

        var task = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
            return Result<TaskDetailResponse>.NotFound("Task not found.");

        return Result<TaskDetailResponse>.Success(new TaskDetailResponse(
            Id:            task.Id,
            Title:         task.Title,
            Description:   task.Description,
            Status:        task.Status,
            Priority:      task.Priority,
            DueDate:       task.DueDate,
            AssigneeId:    task.AssigneeId,
            AssigneeName:  task.Assignee?.FullName,
            CreatedById:   task.CreatedById,
            CreatedByName: task.CreatedBy.FullName,
            ProjectId:     task.ProjectId,
            Comments:      task.Comments.Select(c => new CommentDto(
                c.Id, c.Content, c.AuthorId, c.Author.FullName, c.CreatedAt, c.UpdatedAt
            )).ToList(),
            CreatedAt:  task.CreatedAt,
            UpdatedAt:  task.UpdatedAt
        ));
    }
}