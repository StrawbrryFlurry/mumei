namespace Mumei.DependencyInjection.Core;

public abstract class ApplyBindingConfigurationFactory<TProvider> : Binding<TProvider> {
  protected internal override TProvider Create(IInjector? scope = null) {
    throw new NotSupportedException("Configuration bindings don't define a creation method.");
  }
}