using ManteqTask.Domain.Entities.Requests;
using ManteqTask.Domain.Enums;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Domain.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace ManteqTask.Application.CQRS.Commands.Requests;

public record SubmitRequestCommand(Guid Id) : IRequest<Result<RequestResult>>;

internal sealed class SubmitRequestCommandHandler(
    ICurrentUserService currentUserService,
    ILogger<SubmitRequestCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IRepository<Request> requestRepository)
    : BaseHandler<SubmitRequestCommand, Result<RequestResult>>(currentUserService, logger, unitOfWork)
{
    private readonly IRepository<Request> _requestRepository = requestRepository;

    public override async Task<Result<RequestResult>> Handle(SubmitRequestCommand request, CancellationToken cancellationToken)
    {
        Request? entity = await _requestRepository.GetByIdAsync(request.Id);
        if (entity is null)
            return Error.NotFound("Request not found.");

        // Only the owning doctor may submit.
        if (entity.DoctorId != _currentUser.UserId)
            return Error.Unauthorized("You can only submit your own requests.");

        // Valid transition is only Draft -> Submitted. Anything else (already Submitted / Approved /
        // Rejected) is a duplicate/invalid transition. The xmin concurrency token guards the race
        // where two submits pass this check at once (surfaces as a 409 via the exception handler).
        if (entity.Status != RequestStatus.Draft)
            return Error.Conflict($"Cannot submit a request in {entity.Status} status.");

        entity.Status = RequestStatus.Submitted;
        entity.UpdatedAt = DateTime.UtcNow;

        _requestRepository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        return RequestResult.From(entity);
    }
}
