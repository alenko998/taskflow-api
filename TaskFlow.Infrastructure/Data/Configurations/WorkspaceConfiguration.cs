using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(100);
        builder.Property(w => w.Slug).IsRequired().HasMaxLength(120);
        builder.HasIndex(w => w.Slug).IsUnique();

        builder.HasOne(w => w.Owner)
            .WithMany()
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Members)
            .WithOne(m => m.Workspace)
            .HasForeignKey(m => m.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Projects)
            .WithOne(p => p.Workspace)
            .HasForeignKey(p => p.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Invitations)
            .WithOne(i => i.Workspace)
            .HasForeignKey(i => i.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}