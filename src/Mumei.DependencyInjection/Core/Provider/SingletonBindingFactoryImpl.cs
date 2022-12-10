namespace Mumei.DependencyInjection.Core;

internal class SingletonBindingFactoryImpl : SingletonBindingFactory<object> {
  private readonly object _instance;

  public SingletonBindingFactoryImpl(object instance) {
    _instance = instance;
  }

  protected override object Create(IInjector? scope = null) {
    return _instance;
  }
}