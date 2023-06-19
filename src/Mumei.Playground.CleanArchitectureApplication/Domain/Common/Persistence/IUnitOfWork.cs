namespace CleanArchitectureApplication.Domain.Common.Persistence;

public interface IUnitOfWork {
  public Task CommitAsync(CancellationToken cancellationToken = default);
}