namespace ManteqTask.Domain.Interfaces.Domain.Auditing;

/// <summary>
/// Opt-in marker: entities implementing this are automatically written to the polymorphic
/// AuditTrail on SaveChanges. Empty by design.
/// </summary>
public interface IAuditable
{
}
