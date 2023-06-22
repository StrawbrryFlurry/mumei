namespace Mumei.DependencyInjection.Core;

public sealed class DynamicScopedBinding : ScopedBinding<object> {
  private readonly Func<IInjector, object> _factory;
  private readonly IInjector _injector;

  public DynamicScopedBinding(Func<IInjector, object> factory, IInjector injector) {
    _factory = factory;
    _injector = injector;
  }

  protected internal override object Create(IInjector? scope = null) {
    return _factory(_injector);
  }
}