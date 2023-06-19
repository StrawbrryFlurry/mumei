namespace Mumei.DependencyInjection.Core;

public static partial class ProviderCollectionExtensions {
  public static ProviderCollection Add<TToken, TInstance>(
    this ProviderCollection providers,
    TInstance instance,
    InjectorLifetime lifetime = InjectorLifetime.Singleton
  ) where TInstance : TToken {
    providers.Add(new ProviderDescriptor {
      Token = typeof(TToken),
      ImplementationType = typeof(TInstance),
      ImplementationInstance = instance,
      Lifetime = lifetime
    });

    return providers;
  }

  public static ProviderCollection Add<TInstance>(
    this ProviderCollection providers,
    TInstance instance,
    InjectorLifetime lifetime = InjectorLifetime.Singleton
  ) {
    providers.Add(new ProviderDescriptor {
      Token = typeof(TInstance),
      ImplementationType = typeof(TInstance),
      ImplementationInstance = instance,
      Lifetime = lifetime
    });

    return providers;
  }

  public static ProviderCollection Add(
    this ProviderCollection providers,
    object token,
    object instance,
    InjectorLifetime lifetime = InjectorLifetime.Singleton
  ) {
    providers.Add(new ProviderDescriptor {
      Token = token,
      ImplementationType = instance.GetType(),
      ImplementationInstance = instance,
      Lifetime = lifetime
    });

    return providers;
  }
}