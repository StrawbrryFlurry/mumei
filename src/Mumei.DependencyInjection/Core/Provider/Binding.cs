namespace Mumei.Core.Provider;

public abstract class Binding<TProvider> {
  public abstract TProvider Get(IInjector? scope = null);
  protected abstract TProvider Create(IInjector? scope = null);
}