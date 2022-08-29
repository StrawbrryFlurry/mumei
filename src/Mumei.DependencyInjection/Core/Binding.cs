using System.Runtime.CompilerServices;

namespace Mumei.Core;

public abstract class Binding<TProvider> {
  protected readonly Provider<TProvider> Provider;

  public Type ProviderType = typeof(TProvider);

  public Binding(Provider<TProvider> provider) {
    Provider = provider;
  }

  public abstract TProvider Get(object? scope = null);
}

public class SingletonBinding<TProvider> : Binding<TProvider> {
  private TProvider? _instance;

  public SingletonBinding(Provider<TProvider> provider) : base(provider) { }

  public override TProvider Get(object? scope = null) {
    if (_instance is not null) {
      return _instance;
    }

    _instance = Provider.Get();
    return _instance;
  }
}

public class TransientBinding<TProvider> : Binding<TProvider> {
  public TransientBinding(Provider<TProvider> provider) : base(provider) { }

  public override TProvider Get(object? scope = null) {
    return Provider.Get();
  }
}

public class ScopedBinding<TProvider> : Binding<TProvider> where TProvider : class? {
  private static readonly object GlobalSingletonScope = new();
  private readonly ConditionalWeakTable< /* Injector */ object, TProvider> _instances = new();

  public ScopedBinding(Provider<TProvider> provider) : base(provider) { }

  /// <summary>
  ///   Returns the provider instance scoped to the given injector.
  ///   Provider instances live as long as the injector lives - Usually,
  ///   the injector will be disposed of after a request / or other scope lifetime
  ///   has been completed. If no scope is specified, the global singleton scope is used.
  /// </summary>
  /// <returns></returns>
  public override TProvider Get(object? scope = null) {
    return _instances.GetValue(scope ?? GlobalSingletonScope, _ => Provider.Get());
  }
}