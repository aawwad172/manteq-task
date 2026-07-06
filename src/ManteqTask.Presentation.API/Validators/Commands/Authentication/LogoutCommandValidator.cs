using FluentValidation;
using ManteqTask.Application.CQRS.Commands.Authentication;

namespace ManteqTask.Presentation.API.Validators.Commands.Authentication;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}
