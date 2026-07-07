using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User>            Users            { get; }
    DbSet<Workspace>       Workspaces       { get; }
    DbSet<WorkspaceMember> WorkspaceMembers { get; }
    DbSet<Project>         Projects         { get; }
    DbSet<ProjectMember>   ProjectMembers   { get; }
    DbSet<TaskItem>        Tasks            { get; }
    DbSet<Comment>         Comments         { get; }
    DbSet<Invitation>      Invitations      { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}