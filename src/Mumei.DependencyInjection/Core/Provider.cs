namespace Mumei.Core;

public abstract class Provider<TProvider> : IProviderFactory<TProvider> {
  public abstract TProvider Get();

  public static implicit operator TProvider(Provider<TProvider> provider) {
    return provider.Get();
  }
}