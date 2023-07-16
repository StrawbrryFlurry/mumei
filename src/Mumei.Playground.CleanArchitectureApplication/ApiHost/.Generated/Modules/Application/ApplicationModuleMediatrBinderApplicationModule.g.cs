using Microsoft.CodeAnalysis;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Implementation;
using Mumei.DependencyInjection.Injector.Resolution;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;

namespace CleanArchitectureApplication.Application; 

public sealed class λApplicationModuleλMediatrBinderλIApplicationModule : DynamicInjector {
  public readonly λApplicationModuleλMediatrBinderλIApplicationModuleDynmaicBloom Bloom;

  public λApplicationModuleλMediatrBinderλIApplicationModule(
    IInjector injector
  ) : base(injector, MediatrBinder<IApplicationModule>.Bind) {
    Bloom = new λApplicationModuleλMediatrBinderλIApplicationModuleDynmaicBloom(Providers);
  }
  
  public sealed class λApplicationModuleλMediatrBinderλIApplicationModuleDynmaicBloom : InjectorBloomFilter {
    public λApplicationModuleλMediatrBinderλIApplicationModuleDynmaicBloom(
      IReadOnlyCollection<ProviderDescriptor> providers
    ) {
      foreach (var provider in providers) {
        Add(provider.Token);
      }
    }
  }
}

