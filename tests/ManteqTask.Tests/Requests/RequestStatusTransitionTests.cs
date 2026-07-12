using FluentAssertions;

using ManteqTask.Application.CQRS.Commands.Requests;
using ManteqTask.Domain.Entities.Requests;
using ManteqTask.Domain.Enums;
using ManteqTask.Domain.Results;
using ManteqTask.Tests.Fakes;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace ManteqTask.Tests.Requests;

/// <summary>
/// Exercises the Submit/Approve/Reject command handlers directly, covering the request status
/// state machine (Draft -> Submitted -> Approved/Rejected) and the ownership guard on submit.
/// </summary>
public class RequestStatusTransitionTests
{
    private static readonly Guid DoctorId = Guid.Parse("d0000000-0000-7000-8000-000000000001");
    private static readonly Guid AdminId = Guid.Parse("a0000000-0000-7000-8000-000000000001");

    private static Request SeedRequest(RequestStatus status, Guid? doctorId = null) => new()
    {
        RequestNumber = "PA-2026-00001",
        DoctorId = doctorId ?? DoctorId,
        ProcedureName = "MRI Scan",
        ProcedureDate = new DateOnly(2026, 12, 1),
        EstimatedCost = 1000m,
        Status = status,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    [Fact]
    public async Task Submit_DraftRequest_TransitionsToSubmitted()
    {
        Request request = SeedRequest(RequestStatus.Draft);
        FakeRequestRepository repo = new(request);
        FakeUnitOfWork uow = new();
        FakeCurrentUserService currentUser = new(DoctorId);

        SubmitRequestCommandHandler handler = new(
            currentUser, NullLogger<SubmitRequestCommandHandler>.Instance, uow, repo);

        Result<RequestResult> result = await handler.Handle(new SubmitRequestCommand(request.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(RequestStatus.Submitted));
        request.Status.Should().Be(RequestStatus.Submitted);
        uow.SaveCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Submit_AlreadySubmittedRequest_ReturnsConflictAndDoesNotTransition()
    {
        Request request = SeedRequest(RequestStatus.Submitted);
        FakeRequestRepository repo = new(request);
        FakeUnitOfWork uow = new();
        FakeCurrentUserService currentUser = new(DoctorId);

        SubmitRequestCommandHandler handler = new(
            currentUser, NullLogger<SubmitRequestCommandHandler>.Instance, uow, repo);

        Result<RequestResult> result = await handler.Handle(new SubmitRequestCommand(request.Id), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        request.Status.Should().Be(RequestStatus.Submitted); // unchanged
        uow.SaveCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Approve_SubmittedRequest_TransitionsToApproved()
    {
        Request request = SeedRequest(RequestStatus.Submitted);
        FakeRequestRepository repo = new(request);
        FakeUnitOfWork uow = new();
        FakeCurrentUserService currentUser = new(AdminId);

        ApproveRequestCommandHandler handler = new(
            currentUser, NullLogger<ApproveRequestCommandHandler>.Instance, uow, repo);

        Result<RequestResult> result = await handler.Handle(new ApproveRequestCommand(request.Id, null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(RequestStatus.Approved));
        request.Status.Should().Be(RequestStatus.Approved);
    }

    [Fact]
    public async Task Reject_SubmittedRequest_TransitionsToRejectedWithReason()
    {
        const string reason = "Not medically necessary at this time.";
        Request request = SeedRequest(RequestStatus.Submitted);
        FakeRequestRepository repo = new(request);
        FakeUnitOfWork uow = new();
        FakeCurrentUserService currentUser = new(AdminId);

        RejectRequestCommandHandler handler = new(
            currentUser, NullLogger<RejectRequestCommandHandler>.Instance, uow, repo);

        Result<RequestResult> result = await handler.Handle(new RejectRequestCommand(request.Id, reason), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(RequestStatus.Rejected));
        request.Status.Should().Be(RequestStatus.Rejected);
        request.DecisionReason.Should().Be(reason);
    }

    [Fact]
    public async Task Approve_DraftRequest_ReturnsConflictAndDoesNotTransition()
    {
        Request request = SeedRequest(RequestStatus.Draft);
        FakeRequestRepository repo = new(request);
        FakeUnitOfWork uow = new();
        FakeCurrentUserService currentUser = new(AdminId);

        ApproveRequestCommandHandler handler = new(
            currentUser, NullLogger<ApproveRequestCommandHandler>.Instance, uow, repo);

        Result<RequestResult> result = await handler.Handle(new ApproveRequestCommand(request.Id, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        request.Status.Should().Be(RequestStatus.Draft); // unchanged
        uow.SaveCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Submit_RequestOwnedByAnotherDoctor_ReturnsForbiddenAndDoesNotTransition()
    {
        Guid otherDoctorId = Guid.Parse("e0000000-0000-7000-8000-000000000002");
        Request request = SeedRequest(RequestStatus.Draft, DoctorId);
        FakeRequestRepository repo = new(request);
        FakeUnitOfWork uow = new();
        FakeCurrentUserService currentUser = new(otherDoctorId);

        SubmitRequestCommandHandler handler = new(
            currentUser, NullLogger<SubmitRequestCommandHandler>.Instance, uow, repo);

        Result<RequestResult> result = await handler.Handle(new SubmitRequestCommand(request.Id), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
        request.Status.Should().Be(RequestStatus.Draft); // unchanged
        uow.SaveCallCount.Should().Be(0);
    }
}
