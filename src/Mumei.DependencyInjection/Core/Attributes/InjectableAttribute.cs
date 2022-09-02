using Mumei.Core.Injector;

namespace Mumei.Core.Attributes;

public class InjectableAttribute : Attribute {
  public readonly ProvidedIn ProvidedIn;

  public InjectableAttribute(ProvidedIn providedIn = ProvidedIn.Root) {
    ProvidedIn = providedIn;
  }
}

public class InjectableAttribute<TAppModule> : Attribute {
  public readonly ProvidedIn ProvidedIn;

  public InjectableAttribute(ProvidedIn providedIn = ProvidedIn.Root) {
    ProvidedIn = providedIn;
  }

  public Type AppModuleType => typeof(TAppModule);
}

public enum ProvidedIn {
  /// <summary>
  ///   Provides the injectable type in the root injector
  ///   making it accessible by all modules that are imported by
  ///   the ApplicationModule / ApplicationRoot.
  /// </summary>
  Root,

  /// <summary>
  ///   Provides the injectable type in the platform injector. The platform
  ///   injector is the highest level injector that is shared across all
  ///   application modules.
  /// </summary>
  Platform,

  /// <summary>
  ///   Provides the injectable type in the environment injector.
  ///   The environment injector contains environment specific configuration
  ///   and is the parent of the platform injector. Anything that cannot be
  ///   resolved in the environment injector will throw a <see cref="NullInjectorException" />
  /// </summary>
  Environment
}