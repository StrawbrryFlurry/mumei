using System.Reflection;

namespace Mumei.DependencyInjection.Core;

internal class ComponentRef<TComponent> : IComponentRef<TComponent> where TComponent : IComponent {
  public ComponentRef(Type type, IModuleRef module, Binding<TComponent> instance) {
    Type = type;
    Module = module;
    Instance = instance;
  }

  public Type Type { get; }
  public IModuleRef Module { get; }
  public Binding<TComponent> Instance { get; }

  public IInjector CreateScope() {
    throw new NotImplementedException();
  }

  public IInjector CreateScope(IInjector context) {
    throw new NotImplementedException();
  }

  public object? InvokeWithProviderFactory(IInjector scope, MethodInfo method) {
    throw new NotImplementedException();
  }

  public object? InvokeWithProviderFactory(IInjector scope, object instance, MethodInfo method) {
    throw new NotImplementedException();
  }
}