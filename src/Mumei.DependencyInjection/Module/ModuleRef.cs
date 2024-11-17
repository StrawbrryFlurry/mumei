using System.Diagnostics;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;

namespace Mumei.DependencyInjection.Module;

// TODO: this should not be a class but it's members should be generated into the implementer
public abstract class ModuleRef<TModule> : IModuleRef<TModule> where TModule : IModule {
  public IInjector Parent { get; }
  public Type Type { get; } = typeof(TModule);
  public TModule Injector { get; protected set; }

  public abstract IReadOnlyCollection<IComponentRef<IComponent>> Components { get; }
  public abstract IReadOnlyCollection<IModuleRef<IModule>> Imports { get; }

  public ModuleRef(IInjector parent) {
    Parent = parent;
  }

  [DebuggerHidden]
  [StackTraceHidden]
  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  [DebuggerHidden]
  [StackTraceHidden]
  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return Injector.Get(token, scope, flags);
  }

  public Task<IComponentRef<TComponent>> CreateComponent<TComponent>(IInjector? ctx = null)
    where TComponent : IComponent {
    throw new NotImplementedException();
  }
}