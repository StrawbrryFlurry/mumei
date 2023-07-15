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

[CompilerGenerated]
[MumeiModuleImplFor<IAppModule>]
public sealed class λAppModuleInjector : IAppModule {
  public IInjector Parent { get; }

  internal readonly λApplicationModuleInjector _applicationModule;
  internal readonly λPresentationModuleInjector _presentationModule;
  internal readonly λPersistenceModuleInjector _persistenceModule;
  private readonly λOrderingComponentInjector _orderingComponent;

  public IDomainModule DomainModule { get; }
  public IApplicationModule ApplicationModule => _applicationModule;
  public IPersistenceModule PersistenceModule => _persistenceModule;
  public IPresentationModule PresentationModule => _presentationModule;
  
  public IOrderingComponentComposite Ordering => _orderingComponent;

  public λAppModuleInjector(
    IInjector parent,
    λApplicationModuleInjector applicationModule,
    λPresentationModuleInjector presentationModule,
    λPersistenceModuleInjector persistenceModule,
    λOrderingComponentInjector orderingComponent
  ) {
    Parent = parent;
    
    _applicationModule = applicationModule;
    _presentationModule = presentationModule;
    _persistenceModule = persistenceModule;
    _orderingComponent = orderingComponent;
  }

  public TProvider Get<TProvider>(IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    scope ??= this;
    if ((flags & InjectFlags.SkipSelf) != 0) {
      return Parent.Get(token, scope, flags & ~InjectFlags.SkipSelf);   
    }

    object? instance = null;
    if (TryGet(token, scope, flags, out instance)) {
      return instance;
    }
    
    return Parent.Get(token, scope, flags);
  }

  internal bool TryGet(object token, IInjector scope, InjectFlags flags, out object? instance) {
    if (_applicationModule.TryGet(token, scope, flags, out instance)) {
      return true;
    }
    
    if (_presentationModule.TryGet(token, scope, flags, out instance)) {
      return true;
    }
    
    if (_persistenceModule.TryGet(token, scope, flags, out instance)) {
      return true;
    }
    
    instance = null!;
    return false;
  }
}