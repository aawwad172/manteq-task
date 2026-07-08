using System.Linq.Expressions;

using ManteqTask.Application.CQRS.Commands.Requests;
using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Requests;
using ManteqTask.Domain.Enums;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Domain.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace ManteqTask.Application.CQRS.Queries.Requests;

/// <summary>
/// Lists requests. Carries only filter inputs — the own/all row scope is decided in the handler
/// from the caller's permissions (never from the query), so it cannot be bypassed via query params.
/// </summary>
public record ListRequestsQuery(
    RequestStatus? Status,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo,
    DateOnly? ProcedureFrom,
    DateOnly? ProcedureTo,
    int? PageNumber,
    int? PageSize) : IRequest<Result<PaginationResult<RequestResult>>>;

internal sealed class ListRequestsQueryHandler(
    ICurrentUserService currentUserService,
    ILogger<ListRequestsQueryHandler> logger,
    IUnitOfWork unitOfWork,
    IRepository<Request> requestRepository)
    : BaseHandler<ListRequestsQuery, Result<PaginationResult<RequestResult>>>(currentUserService, logger, unitOfWork)
{
    private readonly IRepository<Request> _requestRepository = requestRepository;

    public override async Task<Result<PaginationResult<RequestResult>>> Handle(ListRequestsQuery query, CancellationToken cancellationToken)
    {
        // Scope decided server-side from the caller's permissions.
        bool viewAll = _currentUser.HasPermission(PermissionConstants.Requests.ViewAll);
        Guid ownerId = _currentUser.UserId;

        RequestStatus? status = query.Status;
        DateOnly? procedureFrom = query.ProcedureFrom;
        DateOnly? procedureTo = query.ProcedureTo;

        // CreatedAt is timestamptz. Filter by whole calendar day inclusively:
        //   from -> start of the day (00:00 UTC), inclusive
        //   to   -> start of the next day (exclusive), so the whole 'to' day is included.
        DateTime? createdFrom = query.CreatedFrom is { } cf
            ? DateTime.SpecifyKind(cf.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : null;
        DateTime? createdToExclusive = query.CreatedTo is { } cto
            ? DateTime.SpecifyKind(cto.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
            : null;

        // Single Expression: the own-scope term is always present (unless the caller can view all),
        // and every filter is applied in the database. CountAsync runs against this same scoped query.
        Expression<Func<Request, bool>> predicate = r =>
            (viewAll || r.DoctorId == ownerId)
            && (status == null || r.Status == status)
            && (createdFrom == null || r.CreatedAt >= createdFrom)
            && (createdToExclusive == null || r.CreatedAt < createdToExclusive)
            && (procedureFrom == null || r.ProcedureDate >= procedureFrom)
            && (procedureTo == null || r.ProcedureDate <= procedureTo);

        PaginationResult<Request> page = await _requestRepository.GetAllAsync(query.PageNumber, query.PageSize, predicate);

        return new PaginationResult<RequestResult>
        {
            Page = (page.Page ?? []).Select(RequestResult.From).ToList(),
            TotalRecords = page.TotalRecords,
            TotalDisplayRecords = page.TotalDisplayRecords,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }
}
