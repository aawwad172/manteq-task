using ManteqTask.Application.CQRS.Commands.Requests;
using ManteqTask.Application.CQRS.Queries.Requests;
using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Results;
using ManteqTask.Presentation.API.Extensions;
using ManteqTask.Presentation.API.Interfaces;
using ManteqTask.Presentation.API.Models;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace ManteqTask.Presentation.API.Routes.Requests;

// Thin HTTP adapter: binds the query-string filters and forwards them. All row-scoping/permission
// logic lives in the handler (via ICurrentUserService), so this touches no HttpContext or claims.
public class ListRequests : IQueryRoute<ListRequestsQuery>
{
    public static async Task<IResult> RegisterRoute(
        [AsParameters] ListRequestsQuery query,
        [FromServices] IMediator mediator)
    {
        Result<PaginationResult<RequestResult>> result = await mediator.Send(query);
        return result.ToApiResult(data =>
            Results.Ok(ApiResponse<PaginationResult<RequestResult>>.SuccessResponse(data)));
    }
}
