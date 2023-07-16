using Mumei.DependencyInjection.Injector.Behavior;

namespace Mumei.DependencyInjection.Injector.Implementation;

/// <summary>
///   The injector instance that is used in scoped
///   bindings to refer to the singleton instance.
/// </summary>
public sealed class SingletonScope : IInjector {
  public static readonly SingletonScope Instance = new();

  public IInjector Parent => default!;

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    throw new NotSupportedException(
      "SingletonScopeInjector does not support dependency resolution and is only used for scoping purposes."
    );
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    throw new NotSupportedException(
      "SingletonScopeInjector does not support dependency resolution and is only used for scoping purposes."
    );
  }
}