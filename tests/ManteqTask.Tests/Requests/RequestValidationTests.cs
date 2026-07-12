using FluentAssertions;

using FluentValidation.Results;

using ManteqTask.Application.CQRS.Commands.Requests;
using ManteqTask.Presentation.API.Validators.Commands.Requests;

using Xunit;

namespace ManteqTask.Tests.Requests;

/// <summary>
/// Exercises the FluentValidation validators directly, where the input business rules live:
/// reject reason length, estimated cost &gt; 0, and procedure date not in the past.
/// </summary>
public class RequestValidationTests
{
    private static DateOnly FutureDate => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
    private static DateOnly PastDate => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

    [Fact]
    public void RejectValidator_ReasonShorterThanTenChars_Fails()
    {
        RejectRequestCommandValidator validator = new();

        ValidationResult result = validator.Validate(new RejectRequestCommand(Guid.NewGuid(), "too short")); // 9 chars

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RejectRequestCommand.Reason));
    }

    [Fact]
    public void CreateValidator_EstimatedCostNotPositive_Fails()
    {
        CreateRequestCommandValidator validator = new();

        ValidationResult result = validator.Validate(new CreateRequestCommand("MRI Scan", FutureDate, 0m));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRequestCommand.EstimatedCost));
    }

    [Fact]
    public void CreateValidator_ProcedureDateInPast_Fails()
    {
        CreateRequestCommandValidator validator = new();

        ValidationResult result = validator.Validate(new CreateRequestCommand("MRI Scan", PastDate, 1000m));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRequestCommand.ProcedureDate));
    }
}
