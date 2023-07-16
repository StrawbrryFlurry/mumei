using System.Collections.Concurrent;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Implementation;

namespace Mumei.DependencyInjection.Providers.Resolution;

public abstract class ScopedBinding<TProvider> : Binding<TProvider> {
  private readonly ConcurrentDictionary<WeakReference<IInjector>, TProvider> _instances
    = new(InjectorWeakReferenceEqualityComparer.Instance);

  /// <summary>
  ///   Returns the provider instance scoped to the given injector.
  ///   Provider instances live as long as the injector lives - Usually,
  ///   the injector will be disposed of after a request / or other scope lifetime
  ///   has been completed. If no scope is specified, the global singleton scope is used.
  /// </summary>
  public override TProvider Get(IInjector? scope = null) {
    scope ??= SingletonScope.Instance;
    return _instances.GetOrAdd(
      new WeakReference<IInjector>(
        scope
      ),
      _ => Create(scope)
    );
  }

  private sealed class InjectorWeakReferenceEqualityComparer : IEqualityComparer<WeakReference<IInjector>> {
    public static readonly InjectorWeakReferenceEqualityComparer Instance = new();

    public bool Equals(WeakReference<IInjector>? x, WeakReference<IInjector>? y) {
      if (x is null || y is null) {
        return false;
      }

      if (!x.TryGetTarget(out var xTarget) || !y.TryGetTarget(out var yTarget)) {
        return false;
      }

      return ReferenceEquals(xTarget, yTarget);
    }

    public int GetHashCode(WeakReference<IInjector> obj) {
      if (!obj.TryGetTarget(out var target)) {
        return 0;
      }

      return target.GetHashCode();
    }
  }
}