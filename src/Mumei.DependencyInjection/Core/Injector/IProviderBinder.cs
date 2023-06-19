namespace Mumei.DependencyInjection.Core;

public interface IProviderBinder {
  public static abstract void Bind(ProviderCollection providers);
}