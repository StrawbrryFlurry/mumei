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
public sealed class λAppModule : IAppModule {
  public IInjector Scope { get; }
  public IInjector Parent { get; }
  
  internal readonly λApplicationModule λApplicationModule;
  
  public IDomainModule DomainModule { get; }
  public IApplicationModule ApplicationModule => λApplicationModule;
  public IPersistenceModule PersistenceModule { get; }
  public IPresentationModule PresentationModule { get; }
  public IOrderingComponentComposite Ordering { get; }

  public λAppModule(IInjector scope) {
    Scope = scope;
  }

  public λAppModule(λApplicationModule applicationModule) {
    λApplicationModule = applicationModule;
  }

  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }


  public IInjector CreateScope() {
    throw new NotImplementedException();
  }

  public IInjector CreateScope(IInjector context) {
    throw new NotImplementedException();
  }
}