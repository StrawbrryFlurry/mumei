namespace Mumei.DependencyInjection.Core;

public sealed record ProviderDescriptor {
  public required object Token { get; init; }
  public Type? ImplementationType { get; init; }

  public object? ImplementationInstance { get; init; }
  public Func<IInjector, object>? ImplementationFactory { get; init; }

  public required InjectorLifetime Lifetime { get; init; }
}