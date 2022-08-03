namespace Mumei.Core;

public interface IModule {
  public T Get<T>();
  public object Get(Type provider);
}