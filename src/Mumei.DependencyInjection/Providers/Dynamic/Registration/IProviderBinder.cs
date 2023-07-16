using Mumei.DependencyInjection.Providers.Registration;

namespace Mumei.DependencyInjection.Providers.Dynamic.Registration;

public interface IProviderBinder {
  public static abstract void Bind(ProviderCollection providers);
}