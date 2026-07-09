using ManteqTask.Domain.Entities.Authentication;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManteqTask.Infrastructure.Configurations.Seed;

public class UsersRolesSeed : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasData([
            // 1. Link the Initial Admin User to the SuperAdmin Role
            new UserRole
            {
                // The Composite Primary Key is UserId and RoleId
                UserId = AuthSeedConstants.InitialAdminUserId,
                RoleId = AuthSeedConstants.RoleIdSuperAdmin,
            },

            // 2. Link the seeded Doctor User to the Doctor Role
            new UserRole
            {
                UserId = AuthSeedConstants.DoctorUserId,
                RoleId = AuthSeedConstants.RoleIdDoctor,
            }
        ]);
    }
}
