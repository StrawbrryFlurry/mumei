using System.Reflection;

namespace Mumei.DependencyInjection.Core;

/// <summary>
///   Wraps a runtime instance of <see cref="IComponent" /> and provides an interface for using it's
///   providers.
/// </summary>
/// <typeparam name="TComponent"></typeparam>
public interface IComponentRef<out TComponent> : IInjector where TComponent : IComponent {
  public Type Type { get; }
  public IModuleRef<IModule> Module { get; }

  public IComponent Injector { get; }

  /// <summary>
  ///   Creates an instance of the component using the provided scope injector for dependency resolution and
  ///   calls the method overload provided using a compile time factory method for creating the method
  ///   arguments.
  /// </summary>
  /// <param name="scope"></param>
  /// <param name="method"></param>
  /// <returns></returns>
  public object? InvokeWithProviderFactory(IInjector scope, MethodInfo method);

  public object? InvokeWithProviderFactory(IInjector scope, object instance, MethodInfo method);
}