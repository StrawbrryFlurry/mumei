using Mumei.DependencyInjection.Injector.Implementation;

namespace Mumei.DependencyInjection.Injector.Registration;

/// <summary>
///   Defines the scope in which an injectable type is provided.
/// </summary>
public enum ProvidedIn {
  /// <summary>
  /// Provides the injectable type in the generated context injector.
  /// The consumer of the DI framework can use this injector to provide
  /// dynamically scoped dependencies, similar to how the Service Provider
  /// handles scoped dependencies. Implementers can also use any arbitrary
  /// injector as the context, as long as it is provided when a dependency
  /// is resolved by an injector.
  /// </summary>
  Context,

  /// <summary>
  /// Provides the injectable type in the next module injector.
  /// </summary>
  Host,

  /// <summary>
  ///   Provides the injectable type in the root injector
  ///   making it accessible by all modules that are imported by
  ///   the ApplicationModule / ApplicationRoot.
  /// </summary>
  Root,

  /// <summary>
  ///   Provides the injectable type in the environment injector.
  ///   The environment injector contains environment specific configuration
  ///   for the current application root.
  /// </summary>
  Environment,

  /// <summary>
  ///   Provides the injectable type in the platform injector. The platform
  ///   injector is shared across all application modules / roots. The platform
  ///   itself provides multiple environments that themselves provide the global
  ///   configuration for an application root.
  ///   Anything that cannot be resolved in the platform injector will
  ///   throw a <see cref="NullInjector.NullInjectorException" />.
  /// </summary>
  Platform
}