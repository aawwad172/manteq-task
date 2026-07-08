using ManteqTask.Domain.Enums;

namespace ManteqTask.Domain.Entities.Auditing;

/// <summary>
/// General-purpose, polymorphic audit record. One row per audited entity change,
/// written automatically on SaveChanges for entities marked IAuditable.
/// </summary>
public class AuditTrail
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The audited entity/table name, e.g. "Request".</summary>
    public required string EntityType { get; set; }

    /// <summary>The Guid primary key of the affected row.</summary>
    public required Guid EntityId { get; set; }

    public required AuditAction Action { get; set; }

    /// <summary>FK to the existing User who made the change. Null for system actions.</summary>
    public Guid? ChangedBy { get; set; }

    public required DateTime ChangedAt { get; set; }

    /// <summary>
    /// jsonb field-level diff: { "Field": { "old": ..., "new": ... }, ... }.
    /// Null when there is nothing to record.
    /// </summary>
    public string? Changes { get; set; }
}
