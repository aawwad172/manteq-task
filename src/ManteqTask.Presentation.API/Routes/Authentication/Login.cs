using FluentValidation;
using FluentValidation.Results;
using ManteqTask.Application.CQRS.Commands.Authentication;
using ManteqTask.Domain.Results;
using ManteqTask.Presentation.API.Extensions;
using ManteqTask.Presentation.API.Interfaces;
using ManteqTask.Presentation.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ManteqTask.Presentation.API.Routes.Authentication;

public class Login : ICommandRoute<LoginCommand>
{
    public static async Task<IResult> RegisterRoute(
        [FromBody] LoginCommand request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<LoginCommand> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

            return Error.Validation(errors).ToErrorResult();
        }

        Result<LoginCommandResult> result = await mediator.Send(request);
        return result.ToApiResult(data =>
            Results.Ok(ApiResponse<LoginCommandResult>.SuccessResponse(data)));
    }
}
