using Mumei.DependencyInjection.Injector.Implementation;

namespace Mumei.DependencyInjection.Injector.Behavior;

[Flags]
public enum InjectFlags {
  /// <summary>
  /// Does not affect the dependency resolution.
  /// </summary>
  None = 1 << 0,

  /// <summary>
  ///   Marks the dependency as optional and injects null if the dependency is not found.
  /// </summary>
  Optional = 1 << 2,

  /// <summary>
  ///   Restricts the dependency resolution to the current injector. If the dependency is
  ///   not found in in this injector a <see cref="NullInjector.NullInjectorException" /> is thrown.
  /// </summary>
  Self = 1 << 3,

  /// <summary>
  ///   Starts the dependency resolution in the direct parent of this injector.
  ///   This will usually be the module injector if the injector is a component.
  /// </summary>
  SkipSelf = 1 << 4,

  /// <summary>
  ///   Restricts the dependency resolution that it can only be resolved by
  ///   the current module injector. If the dependency is not found
  ///   within the current module injector, a <see cref="NullInjector.NullInjectorException" /> is thrown.
  ///   Notes:
  ///     - If both Self and Host are specified, Host takes precedence.
  /// </summary>
  Host = 1 << 5,

  /// <summary>
  ///  Starts the dependency resolution in the parent of the current module injector.
  ///  If the current injector is a module injector, this will do the same as SkipSelf.
  /// </summary>
  SkipHost = 1 << 6
}