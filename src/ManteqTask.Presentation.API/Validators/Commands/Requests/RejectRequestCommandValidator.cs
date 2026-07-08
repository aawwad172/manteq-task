using FluentValidation;

using ManteqTask.Application.CQRS.Commands.Requests;

namespace ManteqTask.Presentation.API.Validators.Commands.Requests;

public class RejectRequestCommandValidator : AbstractValidator<RejectRequestCommand>
{
    public RejectRequestCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters.");
    }
}
