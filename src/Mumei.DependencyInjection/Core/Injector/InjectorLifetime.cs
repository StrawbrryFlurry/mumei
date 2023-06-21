namespace Mumei.DependencyInjection.Core;

public enum InjectorLifetime {
  Singleton,
  Transient,

  /// <summary>
  /// Scopes the provider to the scope injector provided when resolving the provider.
  /// If no scope is provided, the provider instance is scoped to the injector that was used to resolve the provider.
  /// </summary>
  Scoped
}