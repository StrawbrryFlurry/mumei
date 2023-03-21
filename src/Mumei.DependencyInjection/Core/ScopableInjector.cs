namespace Mumei.DependencyInjection.Core;

public interface IScopableInjector : IInjector {
  public IInjector CreateScope();
  public IInjector CreateScope(IInjector context);
}