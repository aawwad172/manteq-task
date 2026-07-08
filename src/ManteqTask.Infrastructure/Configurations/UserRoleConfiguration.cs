using ManteqTask.Domain.Entities.Authentication;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManteqTask.Infrastructure.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // --- 1. Configure M:M Relationship: User <-> Role (via UserRole) ---
        builder.HasKey(ur => new { ur.UserId, ur.RoleId }); // Define Composite PK

        // FKs
        builder.HasOne(x => x.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // avoid cascading deletes if a role is removed accidentally


        // Useful indexes
        builder.HasIndex(x => new { x.UserId, x.RoleId });
    }
}
