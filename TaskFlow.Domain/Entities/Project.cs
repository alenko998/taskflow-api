using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Events;

namespace TaskFlow.Domain.Entities;

public class Project : BaseEntity
{
    public string        Name        { get; private set; } = string.Empty;
    public string        Description { get; private set; } = string.Empty;
    public ProjectStatus Status      { get; private set; } = ProjectStatus.Active;
    public TaskPriority  Priority    { get; private set; } = TaskPriority.Medium;
    public DateTime?     Deadline    { get; private set; }
    public Guid          WorkspaceId { get; private set; }
    public string        CreatedById { get; private set; } = string.Empty;

    public Workspace             Workspace { get; private set; } = null!;
    public User                  CreatedBy { get; private set; } = null!;
    public ICollection<TaskItem> Tasks     { get; private set; } = new List<TaskItem>();
    public ICollection<ProjectMember> Members { get; private set; } = new List<ProjectMember>();

    private Project() { }

    public static Project Create(string name, string description, TaskPriority priority, DateTime? deadline, Guid workspaceId, string createdById)
    {
        var project = new Project
        {
            Name        = name,
            Description = description,
            Priority    = priority,
            Deadline    = deadline,
            WorkspaceId = workspaceId,
            CreatedById = createdById,
        };
        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, name, createdById));
        return project;
    }

    public void Update(string name, string description, TaskPriority priority, DateTime? deadline, ProjectStatus status)
    {
        Name        = name;
        Description = description;
        Priority    = priority;
        Deadline    = deadline;
        Status      = status;
        SetUpdatedAt();
    }
}