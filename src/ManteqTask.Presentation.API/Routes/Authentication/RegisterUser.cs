using FluentValidation;
using FluentValidation.Results;
using ManteqTask.Application.CQRS.Commands.Authentication;
using ManteqTask.Domain.Results;
using ManteqTask.Presentation.API.Extensions;
using ManteqTask.Presentation.API.Interfaces;
using ManteqTask.Presentation.API.Models;
using MediatR;

namespace ManteqTask.Presentation.API.Routes.Authentication;

public class RegisterUser : ICommandRoute<RegisterUserCommand>
{
    public static async Task<IResult> RegisterRoute(
           RegisterUserCommand command,
           IMediator mediator,
           IValidator<RegisterUserCommand> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

            return Error.Validation(errors).ToErrorResult();
        }

        Result<RegisterUserCommandResult> result = await mediator.Send(command);
        return result.ToApiResult(data =>
            Results.Created(
                $"/users/{data.Id}",
                ApiResponse<RegisterUserCommandResult>.SuccessResponse(data)));
    }
}
