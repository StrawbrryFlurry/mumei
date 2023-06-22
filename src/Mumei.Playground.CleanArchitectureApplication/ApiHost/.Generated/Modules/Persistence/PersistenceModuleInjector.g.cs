using CleanArchitectureApplication.Domain.Common.Persistence;
using CleanArchitectureApplication.Persistence;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λPersistenceModuleInjector : IPersistenceModule {
  public IInjector Parent { get; }

  private λIUnitOfworkBinding _iunitOfWorkBinding;

  public IUnitOfWork UnitOfWork => _iunitOfWorkBinding.Get(this);
  public PersistenceOptions Options { get; }

  public λPersistenceModuleInjector(IInjector parent) {
    Parent = parent;
    _iunitOfWorkBinding = new λIUnitOfworkBinding();
  }

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags)!;
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    scope ??= this;
    if (TryGet(token, scope, flags, out var instance)) {
      return instance!;
    }

    return Parent.Get(token, scope, flags);
  }

  public bool TryGet(object token, IInjector? scope, InjectFlags flags, out object? instance) {
    if (ReferenceEquals(token, typeof(IUnitOfWork))) {
      instance = _iunitOfWorkBinding.Get(scope);
      return true;
    }

    instance = null;
    return false;
  }
}