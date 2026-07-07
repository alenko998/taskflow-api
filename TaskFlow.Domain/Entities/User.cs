using Microsoft.AspNetCore.Identity;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName  { get; private set; } = string.Empty;
    public bool IsVerified  { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<WorkspaceMember> WorkspaceMemberships { get; private set; } = new List<WorkspaceMember>();
    public ICollection<ProjectMember>   ProjectMemberships   { get; private set; } = new List<ProjectMember>();
    public ICollection<TaskItem>        AssignedTasks        { get; private set; } = new List<TaskItem>();
    public ICollection<Comment>         Comments             { get; private set; } = new List<Comment>();

    private User() { }

    public static User Create(string firstName, string lastName, string email)
    {
        return new User
        {
            FirstName      = firstName,
            LastName       = lastName,
            Email          = email,
            UserName       = email,
            NormalizedEmail    = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
        };
    }

    public void Verify() => IsVerified = true;

    public string FullName => $"{FirstName} {LastName}";
}