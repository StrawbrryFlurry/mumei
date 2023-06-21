using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Application; 

public sealed class ApplicationModuleλMediatrBinderλIApplicationModule : DynamicProviderBinder {
  public ApplicationModuleλMediatrBinderλIApplicationModule(
    IInjector injector
  ) : base(injector, MediatrBinder<IApplicationModule>.Bind) { }
}