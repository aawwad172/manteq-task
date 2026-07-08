using FluentValidation;
using FluentValidation.Results;
using ManteqTask.Application.CQRS.Commands.Requests;
using ManteqTask.Domain.Results;
using ManteqTask.Presentation.API.Extensions;
using ManteqTask.Presentation.API.Interfaces;
using ManteqTask.Presentation.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ManteqTask.Presentation.API.Routes.Requests;

public class CreateRequest : ICommandRoute<CreateRequestCommand>
{
    public static async Task<IResult> RegisterRoute(
        [FromBody] CreateRequestCommand request,
        [FromServices] IMediator mediator,
        [FromServices] IValidator<CreateRequestCommand> validator)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

            return Error.Validation(errors).ToErrorResult();
        }

        Result<RequestResult> result = await mediator.Send(request);
        return result.ToApiResult(data =>
            Results.Created($"/api/requests/{data.Id}", ApiResponse<RequestResult>.SuccessResponse(data)));
    }
}
