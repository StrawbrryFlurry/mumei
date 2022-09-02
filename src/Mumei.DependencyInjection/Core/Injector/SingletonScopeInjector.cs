namespace Mumei.Core.Injector;

public sealed class SingletonScopeλInjector : IInjector {
  public static readonly SingletonScopeλInjector Instance = new();

  public IInjector Parent => default!;

  public T Get<T>() {
    throw new NotSupportedException(
      "SingletonScopeInjector does not support dependency resolution and is only used for scoping purposes."
    );
  }

  public object Get(Type provider) {
    throw new NotSupportedException(
      "SingletonScopeInjector does not support dependency resolution and is only used for scoping purposes."
    );
  }
}