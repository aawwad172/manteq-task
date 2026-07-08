using FluentValidation;
using FluentValidation.Results;
using ManteqTask.Application.CQRS.Commands.Requests;
using ManteqTask.Domain.Results;
using ManteqTask.Presentation.API.Extensions;
using ManteqTask.Presentation.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ManteqTask.Presentation.API.Routes.Requests;

// Route id is authoritative; overrides any id sent in the body.
public class RejectRequest
{
    public static async Task<IResult> RegisterRoute(
        [FromRoute] Guid id,
        [FromBody] RejectRequestCommand request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<RejectRequestCommand> validator)
    {
        RejectRequestCommand command = request with { Id = id };

        ValidationResult validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

            return Error.Validation(errors).ToErrorResult();
        }

        Result<RequestResult> result = await mediator.Send(command);
        return result.ToApiResult(data =>
            Results.Ok(ApiResponse<RequestResult>.SuccessResponse(data)));
    }
}
