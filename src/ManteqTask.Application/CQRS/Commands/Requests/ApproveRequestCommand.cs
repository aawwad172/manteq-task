using ManteqTask.Domain.Entities.Requests;
using ManteqTask.Domain.Enums;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Domain.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace ManteqTask.Application.CQRS.Commands.Requests;

public record ApproveRequestCommand(Guid Id, string? Reason) : IRequest<Result<RequestResult>>;

internal sealed class ApproveRequestCommandHandler(
    ICurrentUserService currentUserService,
    ILogger<ApproveRequestCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IRepository<Request> requestRepository)
    : BaseHandler<ApproveRequestCommand, Result<RequestResult>>(currentUserService, logger, unitOfWork)
{
    private readonly IRepository<Request> _requestRepository = requestRepository;

    public override async Task<Result<RequestResult>> Handle(ApproveRequestCommand request, CancellationToken cancellationToken)
    {
        Request? entity = await _requestRepository.GetByIdAsync(request.Id);
        if (entity is null)
            return Error.NotFound("Request not found.");

        // Valid only from Submitted. Draft / already Approved / already Rejected are illegal transitions.
        // The xmin concurrency token guards concurrent decisions (surfaces as 409 via the exception handler).
        if (entity.Status != RequestStatus.Submitted)
            return Error.Conflict($"Cannot approve a request in {entity.Status} status.");

        entity.Status = RequestStatus.Approved;
        entity.DecisionReason = request.Reason;   // approval reason is optional
        entity.UpdatedAt = DateTime.UtcNow;

        _requestRepository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        return RequestResult.From(entity);
    }
}
