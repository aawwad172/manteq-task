using ManteqTask.Application.CQRS.Commands.Requests;
using ManteqTask.Domain.Results;
using ManteqTask.Presentation.API.Extensions;
using ManteqTask.Presentation.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ManteqTask.Presentation.API.Routes.Requests;

// Route id is authoritative. Body is optional (approval reason is optional), so it binds as nullable.
public class ApproveRequest
{
    public static async Task<IResult> RegisterRoute(
        [FromRoute] Guid id,
        [FromBody] ApproveRequestCommand? request,
        [FromServices] IMediator mediator)
    {
        ApproveRequestCommand command = request is null
            ? new ApproveRequestCommand(id, null)
            : request with { Id = id };

        Result<RequestResult> result = await mediator.Send(command);
        return result.ToApiResult(data =>
            Results.Ok(ApiResponse<RequestResult>.SuccessResponse(data)));
    }
}
