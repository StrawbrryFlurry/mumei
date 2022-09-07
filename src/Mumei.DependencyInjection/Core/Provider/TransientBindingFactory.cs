namespace Mumei.Core.Provider;

public abstract class TransientBindingFactory<TProvider> : Binding<TProvider> {
  public override TProvider Get(IInjector? scope = null) {
    return Create(scope);
  }
}