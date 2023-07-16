using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Registration;
using Mumei.DependencyInjection.Providers.Dynamic;

namespace Mumei.DependencyInjection.Providers.Registration;

public sealed record ProviderDescriptor {
  public required object Token { get; init; }
  public Type? ImplementationType { get; init; }

  public object? ImplementationInstance { get; init; }

  public DynamicProviderFactory? ImplementationFactory { get; init; }

  public required InjectorLifetime Lifetime { get; init; }
}