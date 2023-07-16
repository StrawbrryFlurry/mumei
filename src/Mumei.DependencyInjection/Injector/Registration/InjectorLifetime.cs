namespace Mumei.DependencyInjection.Injector.Registration;

public enum InjectorLifetime {
  Singleton,
  Transient,

  /// <summary>
  /// Scopes the provider to the scope injector provided when resolving the provider.
  /// If no scope is provided, the provider instance is scoped to the injector that was
  /// used to resolve the provider. Note that in the case of a custom injector implementation,
  /// the scope might fall back to the global singleton injector.
  /// </summary>
  Scoped
}