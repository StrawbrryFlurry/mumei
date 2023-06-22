using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Application; 

public sealed class λApplicationModuleλMediatrBinderλIApplicationModule : DynamicProviderBinder {
  public λApplicationModuleλMediatrBinderλIApplicationModule(
    IInjector injector
  ) : base(injector, MediatrBinder<IApplicationModule>.Bind) { }
}