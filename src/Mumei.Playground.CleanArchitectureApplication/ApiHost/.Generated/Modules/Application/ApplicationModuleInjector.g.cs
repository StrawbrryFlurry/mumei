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
  public IInjector Parent { get; }

  internal readonly DynamicProviderBinder λMediatrBinderλIApplicationModule;
  internal readonly λApplicationModuleλBloom λBloom = new();
  
  internal readonly λOrderingComponentInjector λOrderingComponent;
  
  public IOrderComponent Ordering => λOrderingComponent;
  
  public λApplicationModuleInjector(IInjector parent, λOrderingComponentInjector orderingComponent) {
    Parent = parent;
    λOrderingComponent = orderingComponent;
    λMediatrBinderλIApplicationModule = new λApplicationModuleλMediatrBinderλIApplicationModule(this);
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
    if (λMediatrBinderλIApplicationModule.TryGet(token, out instance)) {
      return true;
    }
    
    if (TryGetOrderingComponentProvider(token, scope, flags, out instance)) {
      return true;
    }
    
    instance = null!;
    return false;
  }

  private bool TryGetOrderingComponentProvider(object token, IInjector scope, InjectFlags flags, out object? instance) {
    if (token == typeof(IOrderComponent)) {
      instance = λOrderingComponent;
      return true;
    }
    
    if (token == typeof(IOrderRepository)) {
      instance = λOrderingComponent.OrderRepositoryBinding.Get(scope);
      return true;
    }
    
    instance = null!;
    return false;
  }

  internal sealed class λApplicationModuleλBloom {
    // This will already be initialized when the module is created
    private static readonly long[] _providerBloomVectors = {
      0b_000000000000000000000000000000,
      0b_000000000000000000000000000000,
      0b_000000000000000000000000000000,
      0b_000000000000000000000000000000,
    };

    private const int _vectorCount = 4;
    private const int _bloomSize = 64 * _vectorCount; // 64 bits (long) per vector * 4 vectors
    private const int _bloomMask = _bloomSize - 1;

    internal static bool Contains(Type token) {
      return Contains(token.GUID.GetHashCode());
    }

    internal static bool Contains(string token) {
      return Contains(token.GetHashCode());
    } 
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Contains(object token) {
      return Contains(token.GetHashCode());
    }
    
    private static bool Contains(int hash) {
      hash = ~hash + 1; // Ensure that the hash is positive
      var mask = 1l << hash;
      var vectorIdx = ~~(hash / 64l);
      var bucketVector = _providerBloomVectors[vectorIdx];

      return (mask & bucketVector) != 0u;
    }

    // Example of what the generator will do
    private static void Add(int hash) {
      var mask = 1 << hash;
      var buckedIdx = (hash & _vectorCount) + 1;
      _providerBloomVectors[buckedIdx] |= mask;
    }
  }
}