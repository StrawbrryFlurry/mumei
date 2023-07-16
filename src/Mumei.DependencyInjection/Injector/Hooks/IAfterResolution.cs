namespace Mumei.DependencyInjection.Injector.Hooks;

public interface IAfterResolution {
  public void AfterResolution(IInjector injector);
}