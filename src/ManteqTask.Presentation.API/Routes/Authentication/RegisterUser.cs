using FluentValidation;
using FluentValidation.Results;
using ManteqTask.Application.CQRS.Commands.Authentication;
using ManteqTask.Domain.Exceptions;
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

            // Throw a custom ValidationException that your middleware will catch
            throw new CustomValidationException("Validation failed", errors);
        }

        RegisterUserCommandResult response = await mediator.Send(command);
        return Results.Created(
            $"/users/{response.Id}",
            ApiResponse<RegisterUserCommandResult>.SuccessResponse(response));
    }
}
