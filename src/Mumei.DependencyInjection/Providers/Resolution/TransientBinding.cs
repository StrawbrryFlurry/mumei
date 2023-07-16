using Mumei.DependencyInjection.Injector;

namespace Mumei.DependencyInjection.Providers.Resolution;

public abstract class TransientBinding<TProvider> : Binding<TProvider> {
  public override TProvider Get(IInjector? scope = null) {
    return Create(scope);
  }
}