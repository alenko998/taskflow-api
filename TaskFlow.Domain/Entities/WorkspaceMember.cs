using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class WorkspaceMember
{
    public Guid          WorkspaceId { get; private set; }
    public string        UserId      { get; private set; } = string.Empty;
    public WorkspaceRole Role        { get; private set; }
    public DateTime      JoinedAt    { get; private set; } = DateTime.UtcNow;

    public Workspace Workspace { get; private set; } = null!;
    public User      User      { get; private set; } = null!;

    private WorkspaceMember() { }

    public static WorkspaceMember Create(Guid workspaceId, string userId, WorkspaceRole role)
    {
        return new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId      = userId,
            Role        = role,
        };
    }

    public void UpdateRole(WorkspaceRole role) => Role = role;
}