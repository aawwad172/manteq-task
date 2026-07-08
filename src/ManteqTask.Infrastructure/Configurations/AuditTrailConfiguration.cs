using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Auditing;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManteqTask.Infrastructure.Configurations;

public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.EntityId).IsRequired();

        builder.Property(a => a.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // DateTime maps to timestamptz by default under Npgsql (UTC).
        builder.Property(a => a.ChangedAt).IsRequired();

        builder.Property(a => a.Changes)
            .HasColumnType("jsonb");

        // Retrieve one entity's history efficiently.
        builder.HasIndex(a => new { a.EntityType, a.EntityId });

        // Nullable FK to the existing User; null out the reference if the user is deleted.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.ChangedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
