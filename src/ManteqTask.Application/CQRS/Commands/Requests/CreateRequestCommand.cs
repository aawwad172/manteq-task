using ManteqTask.Domain.Entities.Requests;
using ManteqTask.Domain.Enums;
using ManteqTask.Domain.Interfaces.Application.Services;
using ManteqTask.Domain.Interfaces.Infrastructure;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;
using ManteqTask.Domain.Results;

using MediatR;

using Microsoft.Extensions.Logging;

namespace ManteqTask.Application.CQRS.Commands.Requests;

public record CreateRequestCommand(
    string ProcedureName,
    DateOnly ProcedureDate,
    decimal EstimatedCost) : IRequest<Result<RequestResult>>;

internal sealed class CreateRequestCommandHandler(
    ICurrentUserService currentUserService,
    ILogger<CreateRequestCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IRepository<Request> requestRepository,
    IRequestNumberGenerator requestNumberGenerator)
    : BaseHandler<CreateRequestCommand, Result<RequestResult>>(currentUserService, logger, unitOfWork)
{
    private readonly IRepository<Request> _requestRepository = requestRepository;
    private readonly IRequestNumberGenerator _requestNumberGenerator = requestNumberGenerator;

    public override async Task<Result<RequestResult>> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
    {
        // RequestNumber comes from the sequence-based generator; never recomputed.
        string requestNumber = await _requestNumberGenerator.NextAsync();
        DateTime now = DateTime.UtcNow;

        Request entity = new()
        {
            RequestNumber = requestNumber,
            DoctorId = _currentUser.UserId,       // acting doctor from the authenticated user, not the body
            ProcedureName = request.ProcedureName,
            ProcedureDate = request.ProcedureDate,
            EstimatedCost = request.EstimatedCost,
            Status = RequestStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _requestRepository.AddAsync(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        return RequestResult.From(entity);
    }
}
