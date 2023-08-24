using Mumei.DependencyInjection.Providers.Registration;

namespace Mumei.DependencyInjection.Providers.Dynamic.Registration;

public interface IProviderBinder {
  public void Bind(ProviderCollection providers);
}