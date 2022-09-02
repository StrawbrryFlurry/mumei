namespace Mumei.Core.Injector;

public sealed class NullInjector : IInjector {
  public static readonly NullInjector Instance = new();

  public IInjector Parent => default!;

  public T Get<T>() {
    return (T)Get(typeof(T));
  }

  public object Get(Type provider) {
    throw new NullInjectorException(provider);
  }
}

public class NullInjectorException : Exception {
  public NullInjectorException(Type type) : base($"Provider of type {type.FullName} not found in scope: {{Scope}}") { }
}