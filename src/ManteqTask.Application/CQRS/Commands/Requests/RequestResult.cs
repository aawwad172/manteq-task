using ManteqTask.Domain.Entities.Requests;

namespace ManteqTask.Application.CQRS.Commands.Requests;

/// <summary>
/// Shared read model returned by the Create/Edit/Submit request commands — all three return
/// the same request view, so one result record is used rather than three identical ones.
/// </summary>
public record RequestResult(
    Guid Id,
    string RequestNumber,
    Guid DoctorId,
    string ProcedureName,
    DateOnly ProcedureDate,
    decimal EstimatedCost,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static RequestResult From(Request request) => new(
        request.Id,
        request.RequestNumber,
        request.DoctorId,
        request.ProcedureName,
        request.ProcedureDate,
        request.EstimatedCost,
        request.Status.ToString(),
        request.CreatedAt,
        request.UpdatedAt);
}
