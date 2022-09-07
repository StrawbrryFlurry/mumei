namespace Mumei.Core.Injector;

public sealed class NullInjector : IInjector {
  public static readonly NullInjector Instance = new();

  public IInjector Parent => default!;

  public T Get<T>(InjectFlags flags = InjectFlags.None) {
    return (T)Get(typeof(T));
  }

  public object Get(object provider, InjectFlags flags = InjectFlags.None) {
    throw new NullInjectorException(provider);
  }
}

public class NullInjectorException : Exception {
  public NullInjectorException(object type) : base(
    $"Provider of type {type.GetType().FullName} not found in scope: {{Scope}}") { }
}