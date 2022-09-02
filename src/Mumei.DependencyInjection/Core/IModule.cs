namespace Mumei.Core;

public interface IModule : IInjector {
  public IInjector CreateScope();
  // public ComponentRef<object>[] Components { get; }
}