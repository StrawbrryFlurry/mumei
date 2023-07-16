using Mumei.DependencyInjection.Injector;

namespace Mumei.DependencyInjection.Module;

/// <summary>
///   Wraps a runtime instance of <see cref="IComponent" /> and provides an interface for using it's
///   providers.
/// </summary>
/// <typeparam name="TComponent"></typeparam>
public interface IComponentRef<out TComponent> : IInjector where TComponent : IComponent {
  public Type Type { get; }
  public IModuleRef<IModule> Module { get; }
  public TComponent Injector { get; }
}