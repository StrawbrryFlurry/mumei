﻿using System.Collections;
using System.Runtime.CompilerServices;
using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Core;
using IOrderComponent = CleanArchitectureApplication.Application.Ordering.IOrderComponent;

namespace CleanArchitectureApplication.Application;

public partial interface IApplicationModule : IModule {
  public IOrderComponent Ordering { get; }  
}

public sealed class λApplicationModuleInjector : IApplicationModule {
  public readonly InjectorBloomFilter Bloom;
  public IInjector Parent { get; }

  private readonly λApplicationModuleλMediatrBinderλIApplicationModule _mediatrBinderλIApplicationModule;
  private readonly λOrderingComponentInjector _orderingComponent;

  public IOrderComponent Ordering => _orderingComponent;

  public λApplicationModuleInjector(IInjector parent, λOrderingComponentInjector orderingComponent) {
    Parent = parent;
    _orderingComponent = orderingComponent;
    _mediatrBinderλIApplicationModule = new λApplicationModuleλMediatrBinderλIApplicationModule(this);
    Bloom = new λApplicationModuleλBloom(_orderingComponent, _mediatrBinderλIApplicationModule);
  }

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    scope ??= this;
    if ((flags & InjectFlags.SkipSelf) != 0) {
      return Parent.Get(token, scope, flags & ~InjectFlags.SkipSelf);
    }

    // Don't check for Host because this is a module host.

    object? instance = null;
    if (TryGet(token, scope, flags, out instance)) {
      return instance;
    }

    return Parent.Get(token, scope, flags);
  }

  internal bool TryGet(object token, IInjector scope, InjectFlags flags, out object? instance) {
    if (!Bloom.Contains(token)) {
      instance = null!;
      return false;
    }

    if (_mediatrBinderλIApplicationModule.TryGet(token, out instance)) {
      return true;
    }

    if (_orderingComponent.TryGet(token, scope, flags, out instance)) {
      return true;
    }

    instance = null!;
    return false;
  }
  
  internal sealed class λApplicationModuleλBloom : InjectorBloomFilter {
    public λApplicationModuleλBloom(
      λOrderingComponentInjector orderingComponent,
      λApplicationModuleλMediatrBinderλIApplicationModule mediatrBinderλIApplicationModule
    ) {
      Merge(orderingComponent.Bloom);
      Merge(mediatrBinderλIApplicationModule.Bloom);
    }
  }
}