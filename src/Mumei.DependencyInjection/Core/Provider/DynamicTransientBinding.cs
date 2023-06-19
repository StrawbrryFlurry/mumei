namespace Mumei.DependencyInjection.Core;

public sealed class DynamicTransientBinding : TransientBinding<object> {
  private readonly Func<IInjector, object> _factory;
  private readonly IInjector _injector;

  public DynamicTransientBinding(Func<IInjector, object> factory, IInjector injector) {
    _factory = factory;
    _injector = injector;
  }

  protected override object Create(IInjector? scope = null) {
    return _factory(_injector);
  }
}