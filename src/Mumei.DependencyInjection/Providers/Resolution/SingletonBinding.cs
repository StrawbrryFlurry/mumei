using Mumei.DependencyInjection.Injector;

namespace Mumei.DependencyInjection.Providers.Resolution;

public abstract class SingletonBinding<TProvider> : Binding<TProvider> {
  private TProvider? _instance;

  public override TProvider Get(IInjector? scope = null) {
    if (_instance is not null) {
      return _instance;
    }

    _instance = Create(scope);
    return _instance;
  }
}