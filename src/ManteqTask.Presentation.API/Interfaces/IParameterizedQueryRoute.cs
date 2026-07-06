using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace ManteqTask.Presentation.API.Interfaces;

public interface IParameterizedQueryRoute<TQuery> where TQuery : notnull
{
    static abstract Task<IResult> RegisterRoute(
    [AsParameters] TQuery query,
    [FromServices] IMediator mediator,
    [FromServices] IValidator<TQuery> validator);
}
