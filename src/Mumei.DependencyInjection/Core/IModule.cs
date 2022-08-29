namespace Mumei.Core;

public interface IModule {
  public ComponentRef<object>[] Components { get; }
  public T Get<T>();
  public object Get(Type provider);
}

public interface IApplicationModule : IModule {
  public IModule[] Modules { get; }
}