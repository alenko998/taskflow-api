namespace TaskFlow.Domain.Entities;

public class ProjectMember
{
    public Guid   ProjectId { get; private set; }
    public string UserId    { get; private set; } = string.Empty;
    public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;

    public Project Project { get; private set; } = null!;
    public User    User    { get; private set; } = null!;

    private ProjectMember() { }

    public static ProjectMember Create(Guid projectId, string userId)
    {
        return new ProjectMember { ProjectId = projectId, UserId = userId };
    }
}