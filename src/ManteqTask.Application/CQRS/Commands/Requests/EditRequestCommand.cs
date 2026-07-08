using ManteqTask.Domain.Entities.Requests;
using ManteqTask.Domain.Enums;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Domain.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace ManteqTask.Application.CQRS.Commands.Requests;

public record EditRequestCommand(
    Guid Id,
    string ProcedureName,
    DateOnly ProcedureDate,
    decimal EstimatedCost) : IRequest<Result<RequestResult>>;

internal sealed class EditRequestCommandHandler(
    ICurrentUserService currentUserService,
    ILogger<EditRequestCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IRepository<Request> requestRepository)
    : BaseHandler<EditRequestCommand, Result<RequestResult>>(currentUserService, logger, unitOfWork)
{
    private readonly IRepository<Request> _requestRepository = requestRepository;

    public override async Task<Result<RequestResult>> Handle(EditRequestCommand request, CancellationToken cancellationToken)
    {
        Request? entity = await _requestRepository.GetByIdAsync(request.Id);
        if (entity is null)
            return Error.NotFound("Request not found.");

        // Only the owning doctor may edit.
        if (entity.DoctorId != _currentUser.UserId)
            return Error.Unauthorized("You can only edit your own requests.");

        // Only Draft requests are editable.
        if (entity.Status != RequestStatus.Draft)
            return Error.Conflict("Only requests in Draft status can be edited.");

        entity.ProcedureName = request.ProcedureName;
        entity.ProcedureDate = request.ProcedureDate;
        entity.EstimatedCost = request.EstimatedCost;
        entity.UpdatedAt = DateTime.UtcNow;

        _requestRepository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        return RequestResult.From(entity);
    }
}
