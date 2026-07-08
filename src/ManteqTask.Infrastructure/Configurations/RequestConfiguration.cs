using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Requests;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManteqTask.Infrastructure.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.HasKey(r => r.Id);

        // Optimistic concurrency via PostgreSQL's system xmin column (no byte[] RowVersion).
        // Npgsql 10 removed the UseXminAsConcurrencyToken() helper; the supported equivalent is
        // mapping a uint concurrency token onto the "xmin" system column. Npgsql's convention
        // recognizes this as a system column, so it produces no migration column.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        builder.Property(r => r.RequestNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.RequestNumber).IsUnique();

        builder.Property(r => r.ProcedureName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ProcedureDate).IsRequired();

        builder.Property(r => r.EstimatedCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.DecisionReason)
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired(false);

        // FK to the existing User (no navigation added to User). Restrict so a doctor
        // with requests cannot be deleted out from under them.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
