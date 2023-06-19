namespace Mumei.DependencyInjection.Core;

/// <summary>
///   Serves as a way to dynamically register providers before the application starts.
///   This is useful for migrations from other DI frameworks or solve the issue where
///   dynamic configuration of the injector needs to happen before the application starts.
/// </summary>
public interface IDynamicModule<TModule> : IModule {
  public void ConfigureModule();

  public void HasProvider(object providerToken);

  public IDynamicModule<TModule> BindSingleton(Type providerToken, Type implementationType);
  public IDynamicModule<TModule> BindSingleton(string providerToken, Type implementationType);
  public IDynamicModule<TModule> BindSingleton(Type providerToken, object instance);

  public IDynamicModule<TModule> BindTransient(Type providerToken, Type implementationType);
  public IDynamicModule<TModule> BindTransient(string providerToken, Type implementationType);
  public IDynamicModule<TModule> BindTransient(Type providerToken, object instance);

  public IDynamicModule<TModule> BindScoped(Type providerToken, Type implementationType);
  public IDynamicModule<TModule> BindScoped(string providerToken, Type implementationType);
  public IDynamicModule<TModule> BindScoped(Type providerToken, object instance);
}