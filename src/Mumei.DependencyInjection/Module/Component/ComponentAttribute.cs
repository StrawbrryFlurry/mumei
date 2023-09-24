using Mumei.DependencyInjection.Providers.Registration;

namespace Mumei.DependencyInjection.Module;

/// <summary>
///   Declares a class declaration as a component.
///   Mumei will automatically register this class in the
///   closest module to the file this class is declared in.
///   <remarks>
///     Partial classes are supported if they are in separate directories,
///     making reliable module resolution impossible.
///   </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class ComponentAttribute : Attribute {
  public ComponentAttribute(string Name) { }
}

/// <summary>
///   Declares that the component of type <typeparamref name="TComponent" /> is part of this module
/// </summary>
/// <typeparam name="TComponent">The component type to add to this module</typeparam>
public sealed class ComponentAttribute<TComponent> : Attribute { }