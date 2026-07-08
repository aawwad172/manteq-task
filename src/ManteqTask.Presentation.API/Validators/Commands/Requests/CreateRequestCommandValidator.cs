using FluentValidation;

using ManteqTask.Application.CQRS.Commands.Requests;

namespace ManteqTask.Presentation.API.Validators.Commands.Requests;

public class CreateRequestCommandValidator : AbstractValidator<CreateRequestCommand>
{
    public CreateRequestCommandValidator()
    {
        RuleFor(x => x.ProcedureName)
            .NotEmpty().WithMessage("Procedure name is required.");

        RuleFor(x => x.EstimatedCost)
            .GreaterThan(0).WithMessage("Estimated cost must be greater than zero.");

        RuleFor(x => x.ProcedureDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Procedure date cannot be in the past.");
    }
}
