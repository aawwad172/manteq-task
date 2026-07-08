namespace ManteqTask.Domain.Interfaces.Infrastructure;

/// <summary>
/// Generates human-readable request numbers (e.g. "PA-2026-00001") from a native
/// PostgreSQL sequence. nextval is atomic and concurrency-safe on its own, so no
/// external transaction or locking is required.
/// </summary>
public interface IRequestNumberGenerator
{
    Task<string> NextAsync();
}
