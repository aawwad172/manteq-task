using ManteqTask.Domain.Entities.Requests;
using ManteqTask.Domain.Enums;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Domain.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace ManteqTask.Application.CQRS.Commands.Requests;

public record RejectRequestCommand(Guid Id, string Reason) : IRequest<Result<RequestResult>>;

internal sealed class RejectRequestCommandHandler(
    ICurrentUserService currentUserService,
    ILogger<RejectRequestCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IRepository<Request> requestRepository)
    : BaseHandler<RejectRequestCommand, Result<RequestResult>>(currentUserService, logger, unitOfWork)
{
    private readonly IRepository<Request> _requestRepository = requestRepository;

    public override async Task<Result<RequestResult>> Handle(RejectRequestCommand request, CancellationToken cancellationToken)
    {
        Request? entity = await _requestRepository.GetByIdAsync(request.Id);
        if (entity is null)
            return Error.NotFound("Request not found.");

        // Valid only from Submitted. Draft / already Approved / already Rejected are illegal transitions.
        // The xmin concurrency token guards concurrent decisions (surfaces as 409 via the exception handler).
        if (entity.Status != RequestStatus.Submitted)
            return Error.Conflict($"Cannot reject a request in {entity.Status} status.");

        entity.Status = RequestStatus.Rejected;
        entity.DecisionReason = request.Reason;   // rejection reason is required (validated at the edge)
        entity.UpdatedAt = DateTime.UtcNow;

        _requestRepository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        return RequestResult.From(entity);
    }
}
