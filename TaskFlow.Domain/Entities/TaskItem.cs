using TaskFlow.Domain.Common;
using TaskFlow.Domain.Events;
using TaskFlow.Domain.Enums;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string      Title       { get; private set; } = string.Empty;
    public string      Description { get; private set; } = string.Empty;
    public TaskStatus  Status      { get; private set; } = TaskStatus.Todo;
    public TaskPriority Priority   { get; private set; } = TaskPriority.Medium;
    public DateTime?   DueDate     { get; private set; }
    public Guid        ProjectId   { get; private set; }
    public string      CreatedById { get; private set; } = string.Empty;
    public string?     AssigneeId  { get; private set; }

    public Project  Project   { get; private set; } = null!;
    public User     CreatedBy { get; private set; } = null!;
    public User?    Assignee  { get; private set; }
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

    private TaskItem() { }

    public static TaskItem Create(string title, string description, TaskPriority priority, DateTime? dueDate, Guid projectId, string createdById, string? assigneeId = null)
    {
        var task = new TaskItem
        {
            Title       = title,
            Description = description,
            Priority    = priority,
            DueDate     = dueDate,
            ProjectId   = projectId,
            CreatedById = createdById,
            AssigneeId  = assigneeId,
        };
        task.AddDomainEvent(new TaskCreatedEvent(task.Id, title, projectId, assigneeId));
        return task;
    }

    public void UpdateStatus(TaskStatus status)
    {
        var old = Status;
        Status = status;
        SetUpdatedAt();
        AddDomainEvent(new TaskStatusChangedEvent(Id, old, status, CreatedById));
    }

    public void Update(string title, string description, TaskPriority priority, DateTime? dueDate, string? assigneeId)
    {
        Title       = title;
        Description = description;
        Priority    = priority;
        DueDate     = dueDate;
        AssigneeId  = assigneeId;
        SetUpdatedAt();
    }
}