using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.Hooks;

public interface IAfterResolution {
  public void AfterResolution(IInjector injector);
}