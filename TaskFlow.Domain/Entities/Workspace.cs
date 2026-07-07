using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name    { get; private set; } = string.Empty;
    public string Slug    { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;

    public User                       Owner   { get; private set; } = null!;
    public ICollection<WorkspaceMember> Members  { get; private set; } = new List<WorkspaceMember>();
    public ICollection<Project>         Projects { get; private set; } = new List<Project>();
    public ICollection<Invitation>      Invitations { get; private set; } = new List<Invitation>();

    private Workspace() { }

    public static Workspace Create(string name, string ownerId)
    {
        var slug = name.ToLower().Replace(" ", "-");
        return new Workspace
        {
            Name    = name,
            Slug    = slug,
            OwnerId = ownerId,
        };
    }

    public void UpdateName(string name)
    {
        Name = name;
        Slug = name.ToLower().Replace(" ", "-");
        SetUpdatedAt();
    }
}