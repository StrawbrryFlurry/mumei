namespace Mumei.Core;

public abstract class Provider<TProvider> {
  public abstract TProvider Get();

  public static implicit operator TProvider(Provider<TProvider> provider) {
    return provider.Get();
  }
}