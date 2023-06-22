using CleanArchitectureApplication.Domain.Common.Persistence;

namespace CleanArchitectureApplication.Persistence.Common;

public class UnitOfWork : IUnitOfWork {
  public Task CommitAsync(CancellationToken cancellationToken = default) {
    return Task.CompletedTask;
  }
}