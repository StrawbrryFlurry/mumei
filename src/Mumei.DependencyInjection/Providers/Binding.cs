using System.Diagnostics;
using Mumei.DependencyInjection.Injector;

namespace Mumei.DependencyInjection.Providers;

/// <summary>
/// The basic primitive for creating a provider instance.
/// Bindings provide a common interface for creating any
/// provider instance for a specific injector scope.
/// </summary>
public abstract class Binding {
  /// <inheritdoc cref="Binding{TProvider}.Get"/>
  /// Prefer to use the generic version of this method, if possible.
  /// This only exists for use cases where the provider type is not known
  /// at compile time and must be resolved at runtime.
  public abstract object GetInstance(IInjector scope = null!);
}

/// <inheritdoc cref="Binding"/>
/// <typeparam name="TProvider">The provider type this binding will create</typeparam>
public abstract class Binding<TProvider> : Binding {
  /// <summary>
  /// Retrieves a provider instance for the specified scope. If no instance
  /// for the requested scope exists, a new instance will be created.
  /// </summary>
  /// <param name="scope">The scope for which a provider instance should be retrieved</param>
  /// <returns>A provider instance in the specified scope</returns>
  public abstract TProvider Get(IInjector? scope = null);

  /// <summary>
  /// Abstracts the creation of a provider instance for the specified scope.
  /// The binding implementation will ensure that this method is only called,
  /// if no existing instance for that scope exists, or the lifetime of the
  /// provider instance requires the creation of a new instance.
  /// </summary>
  /// <param name="scope">The scope for which to create a binding instance</param>
  /// <returns>The provider instance, created for the specified scope</returns>
  protected internal abstract TProvider Create(IInjector? scope = null);

  [DebuggerHidden]
  [StackTraceHidden]
  public override object GetInstance(IInjector? scope = null) {
    return Get(scope)!;
  }
}