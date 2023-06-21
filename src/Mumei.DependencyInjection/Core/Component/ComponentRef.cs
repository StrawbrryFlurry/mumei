using System.Reflection;

namespace Mumei.DependencyInjection.Core;

public abstract class ComponentRef<TComponent> : IComponentRef<TComponent> where TComponent : IComponent {
  public IInjector Parent { get; }

  public Type Type { get; }

  public IModuleRef<IModule> Module { get; }

  public Binding<TComponent> Instance { get; }

  public IComponent Injector { get; protected set; }

  public ComponentRef(IInjector parent) {
    Parent = parent;
  }

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return Injector.Get(token, scope, flags);
  }

  public object? InvokeWithProviderFactory(IInjector scope, MethodInfo method) {
    throw new NotImplementedException();
  }

  public object? InvokeWithProviderFactory(IInjector scope, object instance, MethodInfo method) {
    throw new NotImplementedException();
  }
}