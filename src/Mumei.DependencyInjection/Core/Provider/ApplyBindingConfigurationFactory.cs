namespace Mumei.Core.Provider;

public abstract class ApplyBindingConfigurationFactory<TProvider> : Binding<TProvider> {
  protected override TProvider Create(IInjector? scope = null) {
    throw new NotSupportedException("Configuration bindings don't define a creation method.");
  }
}