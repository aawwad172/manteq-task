using ManteqTask.Domain.Entities.Authentication;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManteqTask.Infrastructure.Configurations.Seed;

public class RolesPermissionsSeed : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasData([
            // -----------------------------------------------------------------------------------
            // 1. Doctor Role Permissions
            // -----------------------------------------------------------------------------------
            new RolePermission
            {
                RoleId = AuthSeedConstants.RoleIdDoctor,
                PermissionId = AuthSeedConstants.PermissionIdRequestsCreate
            },
            new RolePermission
            {
                RoleId = AuthSeedConstants.RoleIdDoctor,
                PermissionId = AuthSeedConstants.PermissionIdRequestsEdit
            },
            new RolePermission
            {
                RoleId = AuthSeedConstants.RoleIdDoctor,
                PermissionId = AuthSeedConstants.PermissionIdRequestsSubmit
            },
            new RolePermission
            {
                RoleId = AuthSeedConstants.RoleIdDoctor,
                PermissionId = AuthSeedConstants.PermissionIdRequestsViewOwn
            },

            // -----------------------------------------------------------------------------------
            // 2. Admin Role Permissions (request workflow)
            // -----------------------------------------------------------------------------------
            new RolePermission
            {
                RoleId = AuthSeedConstants.RoleIdAdmin,
                PermissionId = AuthSeedConstants.PermissionIdRequestsViewAll
            },
            new RolePermission
            {
                RoleId = AuthSeedConstants.RoleIdAdmin,
                PermissionId = AuthSeedConstants.PermissionIdRequestsApprove
            },
            new RolePermission
            {
                RoleId = AuthSeedConstants.RoleIdAdmin,
                PermissionId = AuthSeedConstants.PermissionIdRequestsReject
            },
            new RolePermission
            {
                RoleId = AuthSeedConstants.RoleIdAdmin,
                PermissionId = AuthSeedConstants.PermissionIdAuditView
            }
        ]);
    }
}
