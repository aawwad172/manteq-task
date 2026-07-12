using ManteqTask.Domain.Interfaces.Infrastructure.IRepositories;

namespace ManteqTask.Tests.Fakes;

/// <summary>
/// No-op <see cref="IUnitOfWork"/> that only counts <see cref="SaveAsync"/> calls, letting a test
/// assert whether a handler persisted (transition) or short-circuited (guard hit, nothing saved).
/// </summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCallCount { get; private set; }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        SaveCallCount++;
        return Task.FromResult(1);
    }

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Detach<TEntity>(TEntity entity) where TEntity : class { }

    public void Dispose() { }
}
