namespace Mumei.DependencyInjection.Core;

internal sealed class SingletonBindingImpl : SingletonBinding<object> {
  private readonly object _instance;

  public SingletonBindingImpl(object instance) {
    _instance = instance;
  }

  protected override object Create(IInjector? scope = null) {
    return _instance;
  }
}