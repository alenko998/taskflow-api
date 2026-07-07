using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Workspace>       Workspaces       => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
    public DbSet<Project>         Projects         => Set<Project>();
    public DbSet<ProjectMember>   ProjectMembers   => Set<ProjectMember>();
    public DbSet<TaskItem>        Tasks            => Set<TaskItem>();
    public DbSet<Comment>         Comments         => Set<Comment>();
    public DbSet<Invitation>      Invitations      => Set<Invitation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}