using ManteqTask.Domain.Enums;
using ManteqTask.Domain.Interfaces.Domain.Auditing;

namespace ManteqTask.Domain.Entities.Requests;

public class Request : IAuditable
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>Human-readable identifier, e.g. "PA-2026-00001". Unique.</summary>
    public required string RequestNumber { get; set; }

    /// <summary>FK to the existing User who owns the request.</summary>
    public required Guid DoctorId { get; set; }

    public required string ProcedureName { get; set; }

    /// <summary>Scheduled procedure date. Not-in-the-past is enforced in business logic, not the schema.</summary>
    public required DateOnly ProcedureDate { get; set; }

    /// <summary>Must be &gt; 0 (enforced in business logic).</summary>
    public required decimal EstimatedCost { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Draft;

    /// <summary>Required on reject, optional on approve (enforced in business logic).</summary>
    public string? DecisionReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
