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

  internal readonly λApplicationModuleInjector λApplicationModule;
  internal readonly λPresentationModuleInjector λPresentationModule;
  internal readonly λPersistenceModuleInjector λPersistenceModule;
  
  public IDomainModule DomainModule { get; }
  public IApplicationModule ApplicationModule => λApplicationModule;
  public IPersistenceModule PersistenceModule { get; }
  public IPresentationModule PresentationModule { get; }
  public IOrderingComponentComposite Ordering { get; }

  public λAppModuleInjector(
    IInjector parent,
    λApplicationModuleInjector applicationModule,
    λPresentationModuleInjector presentationModule,
    λPersistenceModuleInjector persistenceModule
  ) {
    Parent = parent;
    
    λApplicationModule = applicationModule;
    λPresentationModule = presentationModule;
    λPersistenceModule = persistenceModule;
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
    if (λApplicationModule.TryGet(token, scope, flags, out instance)) {
      return true;
    }
    
    if (λPresentationModule.TryGet(token, scope, flags, out instance)) {
      return true;
    }
    
    if (λPersistenceModule.TryGet(token, scope, flags, out instance)) {
      return true;
    }
    
    instance = null!;
    return false;
  }
}