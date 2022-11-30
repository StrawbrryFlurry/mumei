using Mumei.Core;

namespace Mumei.Hooks; 

public interface IAfterResolution {
  public void AfterResolution(IInjector injector);
}