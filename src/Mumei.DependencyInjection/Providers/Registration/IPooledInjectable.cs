namespace Mumei.DependencyInjection.Providers.Registration;

public interface IPooledInjectable {
  public ValueTask InitializeAsync();
  public ValueTask DisposeAsync();
}