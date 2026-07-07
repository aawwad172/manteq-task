using FluentValidation;
using FluentValidation.Results;
using ManteqTask.Application.CQRS.Commands.Authentication;
using ManteqTask.Domain.Results;
using ManteqTask.Presentation.API.Extensions;
using ManteqTask.Presentation.API.Interfaces;
using ManteqTask.Presentation.API.Models;
using MediatR;

namespace ManteqTask.Presentation.API.Routes.Authentication;

public class RefreshToken : ICommandRoute<RefreshTokenCommand>
{
    public static async Task<IResult> RegisterRoute(
                   RefreshTokenCommand command,
                   IMediator mediator,
                   IValidator<RefreshTokenCommand> validator)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

            return Error.Validation(errors).ToErrorResult();
        }

        Result<RefreshTokenCommandResult> result = await mediator.Send(command);
        return result.ToApiResult(data =>
            Results.Ok(ApiResponse<RefreshTokenCommandResult>.SuccessResponse(data)));
    }
}
