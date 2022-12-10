namespace Mumei.DependencyInjection.Core;

/// <summary>
///   Regular modules provide static analysis of providers that can be
///   retrieved from it. This provides a fast and safe interface for
///   getting provider instances. In some cases though, dynamic resolution
///   is required at runtime, when not all participants of the graph are known
///   prior to startup. GetInstance will try to resolve the provider instance.
///   // TODO Can we do static analysis on components like controllers?
/// </summary>
public interface IDynamicModule {
  public void ConfigureModule();
}