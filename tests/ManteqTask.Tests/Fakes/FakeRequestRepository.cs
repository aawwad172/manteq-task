using System.Linq.Expressions;

using ManteqTask.Domain.Entities;
using ManteqTask.Domain.Entities.Requests;
using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;

namespace ManteqTask.Tests.Fakes;

/// <summary>
/// Dictionary-backed <see cref="IRepository{Request}"/>. Handlers mutate the entity returned by
/// <see cref="GetByIdAsync"/> in place, so the seeded instance reflects any transition — a test can
/// assert on it directly to prove the status did (or did not) change.
/// </summary>
internal sealed class FakeRequestRepository : IRepository<Request>
{
    private readonly Dictionary<Guid, Request> _store = new();

    public FakeRequestRepository(params Request[] seed)
    {
        foreach (Request request in seed)
            _store[request.Id] = request;
    }

    public Task<Request?> GetByIdAsync(Guid id)
        => Task.FromResult(_store.TryGetValue(id, out Request? request) ? request : null);

    public Task<Request> AddAsync(Request entity)
    {
        _store[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Request? Update(Request entity)
    {
        _store[entity.Id] = entity;
        return entity;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }

    public Task<PaginationResult<Request>> GetAllAsync(
        int? pageNumber,
        int? pageSize,
        Expression<Func<Request, bool>>? filter)
        => throw new NotImplementedException("List queries are not exercised by these tests.");
}
