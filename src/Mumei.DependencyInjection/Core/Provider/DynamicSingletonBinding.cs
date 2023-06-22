namespace Mumei.DependencyInjection.Core;

public sealed class DynamicSingletonBinding : SingletonBinding<object> {
  private readonly object _instance;

  public DynamicSingletonBinding(object instance) {
    _instance = instance;
  }

  protected internal override object Create(IInjector? scope = null) {
    return _instance;
  }
}