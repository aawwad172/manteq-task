using ManteqTask.Domain.Entities.Authentication;
using ManteqTask.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManteqTask.Infrastructure.Configurations.Seed;

public class PermissionsSeed : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        // Todo: Add the desired Permissions.
        builder.HasData([
            // --- Request workflow permissions ---
            new Permission
            {
                Id = AuthSeedConstants.PermissionIdRequestsCreate,
                Name = PermissionConstants.Requests.Create,
                Description = "Create a Draft request",
                CreatedAt = AuthSeedConstants.SeedDateUtc,
                CreatedBy = AuthSeedConstants.SystemUserId
            },
            new Permission
            {
                Id = AuthSeedConstants.PermissionIdRequestsEdit,
                Name = PermissionConstants.Requests.Edit,
                Description = "Edit a Draft request",
                CreatedAt = AuthSeedConstants.SeedDateUtc,
                CreatedBy = AuthSeedConstants.SystemUserId
            },
            new Permission
            {
                Id = AuthSeedConstants.PermissionIdRequestsSubmit,
                Name = PermissionConstants.Requests.Submit,
                Description = "Submit a Draft for review",
                CreatedAt = AuthSeedConstants.SeedDateUtc,
                CreatedBy = AuthSeedConstants.SystemUserId
            },
            new Permission
            {
                Id = AuthSeedConstants.PermissionIdRequestsViewOwn,
                Name = PermissionConstants.Requests.ViewOwn,
                Description = "View only the user's own requests",
                CreatedAt = AuthSeedConstants.SeedDateUtc,
                CreatedBy = AuthSeedConstants.SystemUserId
            },
            new Permission
            {
                Id = AuthSeedConstants.PermissionIdRequestsViewAll,
                Name = PermissionConstants.Requests.ViewAll,
                Description = "View all requests (with status/date filters)",
                CreatedAt = AuthSeedConstants.SeedDateUtc,
                CreatedBy = AuthSeedConstants.SystemUserId
            },
            new Permission
            {
                Id = AuthSeedConstants.PermissionIdRequestsApprove,
                Name = PermissionConstants.Requests.Approve,
                Description = "Approve a Submitted request",
                CreatedAt = AuthSeedConstants.SeedDateUtc,
                CreatedBy = AuthSeedConstants.SystemUserId
            },
            new Permission
            {
                Id = AuthSeedConstants.PermissionIdRequestsReject,
                Name = PermissionConstants.Requests.Reject,
                Description = "Reject a Submitted request (reason required)",
                CreatedAt = AuthSeedConstants.SeedDateUtc,
                CreatedBy = AuthSeedConstants.SystemUserId
            },
            new Permission
            {
                Id = AuthSeedConstants.PermissionIdAuditView,
                Name = PermissionConstants.Audit.View,
                Description = "View the status-change audit trail",
                CreatedAt = AuthSeedConstants.SeedDateUtc,
                CreatedBy = AuthSeedConstants.SystemUserId
            }
        ]);
    }
}
