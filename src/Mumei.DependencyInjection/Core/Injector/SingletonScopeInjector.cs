namespace Mumei.DependencyInjection.Core;

/// <summary>
///   The injector instance that is used in scoped
///   binding factories to refer to the singleton instance.
/// </summary>
public sealed class SingletonScopeλInjector : IInjector {
  public static readonly SingletonScopeλInjector Instance = new();

  public IInjector Parent => default!;

  public object Get(object providerToken, InjectFlags flags) {
    throw new NotSupportedException(
      "SingletonScopeInjector does not support dependency resolution and is only used for scoping purposes."
    );
  }

  public T Get<T>(InjectFlags flags) {
    throw new NotSupportedException(
      "SingletonScopeInjector does not support dependency resolution and is only used for scoping purposes."
    );
  }
}