using System.Collections.Concurrent;

namespace Mumei.DependencyInjection.Core;

public delegate Binding<TProvider>? LateBoundProviderBindingAccessor<TProvider>(IInjector scope);

public sealed class LateBoundBinding<TProvider> : ScopedBinding<TProvider> {
  private readonly LateBoundProviderBindingAccessor<TProvider> _accessor;

  private ConcurrentDictionary<WeakReference<IInjector>, Binding<TProvider>> _bindingCache = new();

  public LateBoundBinding(LateBoundProviderBindingAccessor<TProvider> accessor) {
    _accessor = accessor;
  }

  private Binding<TProvider> LoadBindingInstance(IInjector scopeInjector) {
    var instance = _accessor(scopeInjector);
    if (instance is not null) {
      return instance;
    }

    throw new InvalidOperationException(
      $"Cannot access late bound binding of type {typeof(TProvider).FullName} in {scopeInjector.GetType().FullName} before the dependency tree has been realized."
    );
  }

  protected internal override TProvider Create(IInjector? scope = null) {
    scope ??= SingletonScopeλInjector.Instance;
    return _bindingCache.GetOrAdd(new WeakReference<IInjector>(scope), _ => LoadBindingInstance(scope)).Get(scope);
  }
}