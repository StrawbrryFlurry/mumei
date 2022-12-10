namespace Mumei.DependencyInjection.Core;

public interface IDynamicProvider<out TProvider> {
  public TProvider Provide();
}

public interface IDynamicAsyncProvider<TProvider> {
  public Task<TProvider> ProvideAsync();
}