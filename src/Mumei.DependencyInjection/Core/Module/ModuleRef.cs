namespace Mumei.DependencyInjection.Core;

public abstract class ModuleRef<TModule> : IModuleRef<TModule> where TModule : IModule {
  public IInjector Parent { get; }
  public Type Type { get; }
  public IModule Injector { get; protected set; }

  public abstract IReadOnlyCollection<IComponentRef<IComponent>> Components { get; }
  public abstract IReadOnlyCollection<IModuleRef<IModule>> Imports { get; }

  public ModuleRef(IInjector parent) {
    Parent = parent;
  }

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return Injector.Get(token, scope, flags);
  }

  public Task<IComponentRef<TComponent>> CreateComponent<TComponent>(IInjector? ctx = null)
    where TComponent : IComponent {
    throw new NotImplementedException();
  }
}