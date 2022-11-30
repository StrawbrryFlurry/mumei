namespace Mumei.Core;

public interface IModule : IInjector {
  public IInjector CreateScope();
  public IInjector CreateScope(IInjector context);
}