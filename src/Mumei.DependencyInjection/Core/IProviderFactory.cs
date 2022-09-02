namespace Mumei.Core;

public interface IProviderFactory<out TProvider> {
  public TProvider Get();
}