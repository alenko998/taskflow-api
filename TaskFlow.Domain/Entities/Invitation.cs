using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class Invitation : BaseEntity
{
    public string           Email       { get; private set; } = string.Empty;
    public Guid             WorkspaceId { get; private set; }
    public string           InvitedById { get; private set; } = string.Empty;
    public WorkspaceRole    Role        { get; private set; }
    public InvitationStatus Status      { get; private set; } = InvitationStatus.Pending;
    public string           Token       { get; private set; } = string.Empty;
    public DateTime         ExpiresAt   { get; private set; }

    public Workspace Workspace { get; private set; } = null!;
    public User      InvitedBy { get; private set; } = null!;

    private Invitation() { }

    public static Invitation Create(string email, Guid workspaceId, string invitedById, WorkspaceRole role)
    {
        return new Invitation
        {
            Email       = email,
            WorkspaceId = workspaceId,
            InvitedById = invitedById,
            Role        = role,
            Token       = Guid.NewGuid().ToString("N"),
            ExpiresAt   = DateTime.UtcNow.AddDays(7),
        };
    }

    public void Accept()  { Status = InvitationStatus.Accepted; SetUpdatedAt(); }
    public void Decline() { Status = InvitationStatus.Declined; SetUpdatedAt(); }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}