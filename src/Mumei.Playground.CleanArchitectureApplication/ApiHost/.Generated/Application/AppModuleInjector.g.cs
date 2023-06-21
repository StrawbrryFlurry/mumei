using System.Runtime.CompilerServices;
using CleanArchitectureApplication.ApiHost;
using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Application;
using CleanArchitectureApplication.Domain;
using CleanArchitectureApplication.Persistence;
using CleanArchitectureApplication.Presentation;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Internal;

namespace CleanArchitectureApplication.ApiHost;

public partial interface IAppModule : IModule {
  public IDomainModule DomainModule { get; }
  public IApplicationModule ApplicationModule { get; }
  public IPersistenceModule PersistenceModule { get; }
  public IPresentationModule PresentationModule { get; }
  
  public IOrderingComponentComposite Ordering { get; }
}

[CompilerGenerated]
[MumeiModuleImplFor<IAppModule>]
public sealed class λAppModuleInjector : IAppModule {
  public IInjector Parent { get; }

  internal readonly λApplicationModuleInjector λApplicationModule;
  
  public IDomainModule DomainModule { get; }
  public IApplicationModule ApplicationModule => λApplicationModule;
  public IPersistenceModule PersistenceModule { get; }
  public IPresentationModule PresentationModule { get; }
  public IOrderingComponentComposite Ordering { get; }

  public λAppModuleInjector(λApplicationModuleInjector applicationModule) {
    λApplicationModule = applicationModule;
  }

  public TProvider Get<TProvider>(IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    var token = typeof(TProvider);
    if (λApplicationModule.λBloom.MightContainProvider(token)) {
      var instance = λApplicationModule.Get<TProvider>(scope, flags);
    }
    
    throw new NotImplementedException();
  }

  public object Get(object token, IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }
}