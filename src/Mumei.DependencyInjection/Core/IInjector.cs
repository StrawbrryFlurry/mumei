namespace Mumei.Core;

public interface IInjector {
  public IInjector Parent { get; }
  public T Get<T>( /*InjectFlags flags = InjectFlags.None*/);
  public object Get(Type provider);
}