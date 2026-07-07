using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Email).IsRequired().HasMaxLength(256);
        builder.Property(i => i.Token).IsRequired().HasMaxLength(64);
        builder.Property(i => i.Role).HasConversion<string>();
        builder.Property(i => i.Status).HasConversion<string>();
        builder.HasIndex(i => i.Token).IsUnique();

        builder.HasOne(i => i.InvitedBy)
            .WithMany()
            .HasForeignKey(i => i.InvitedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}