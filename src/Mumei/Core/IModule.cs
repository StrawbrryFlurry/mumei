namespace Mumei.Core;

public interface IModule {
  public abstract void Providers();
  public abstract void Imports();
  public abstract void Component();
}