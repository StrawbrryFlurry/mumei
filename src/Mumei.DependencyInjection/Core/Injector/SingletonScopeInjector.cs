namespace Mumei.DependencyInjection.Core;

/// <summary>
///   The injector instance that is used in scoped
///   bindings to refer to the singleton instance.
/// </summary>
public sealed class SingletonScopeλInjector : IInjector {
  public static readonly SingletonScopeλInjector Instance = new();

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