using ManteqTask.Application.CQRS.Commands.Requests;
using ManteqTask.Domain.Results;
using ManteqTask.Presentation.API.Extensions;
using ManteqTask.Presentation.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ManteqTask.Presentation.API.Routes.Requests;

// No request body — the id comes from the route and the acting doctor from the authenticated user.
public class SubmitRequest
{
    public static async Task<IResult> RegisterRoute(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        Result<RequestResult> result = await mediator.Send(new SubmitRequestCommand(id));
        return result.ToApiResult(data =>
            Results.Ok(ApiResponse<RequestResult>.SuccessResponse(data)));
    }
}
