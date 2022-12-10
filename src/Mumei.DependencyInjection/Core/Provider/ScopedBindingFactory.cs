using System.Runtime.CompilerServices;

namespace Mumei.DependencyInjection.Core;

public abstract class ScopedBindingFactory<TProvider> : Binding<TProvider> where TProvider : class {
  private readonly ConditionalWeakTable<IInjector, TProvider> _instances = new();

  /// <summary>
  ///   Returns the provider instance scoped to the given injector.
  ///   Provider instances live as long as the injector lives - Usually,
  ///   the injector will be disposed of after a request / or other scope lifetime
  ///   has been completed. If no scope is specified, the global singleton scope is used.
  /// </summary>
  public override TProvider Get(IInjector? scope = null) {
    return _instances.GetValue(scope ?? SingletonScopeλInjector.Instance, Create);
  }
}