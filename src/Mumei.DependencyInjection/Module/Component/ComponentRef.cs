using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;

namespace Mumei.DependencyInjection.Module;

public abstract class ComponentRef<TComponent> : IComponentRef<TComponent> where TComponent : IComponent {
  public IInjector Parent { get; }
  public Type Type { get; }
  public IModuleRef<IModule> Module { get; }
  public TComponent Injector { get; protected set; }

  public ComponentRef(IInjector parent) {
    Parent = parent;
  }

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return Injector.Get(token, scope, flags);
  }
}