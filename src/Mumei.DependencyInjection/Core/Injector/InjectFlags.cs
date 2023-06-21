namespace Mumei.DependencyInjection.Core;

[Flags]
public enum InjectFlags {
  None = 1 << 0,
  Optional = 1 << 2,
  Self = 1 << 3,
  SkipSelf = 1 << 4,
  Host = 1 << 5
}

public class InjectBehaviorAttribute : Attribute { }

/// <summary>
///   Marks the dependency as optional and injects null if the dependency is not found.
/// </summary>
public class OptionalAttribute : InjectBehaviorAttribute { }

/// <summary>
///   Restricts the dependency resolution to the current injector. If the dependency is
///   not found in in this injector a <see cref="NullInjectorException" /> is thrown.
/// </summary>
public class SelfAttribute : InjectBehaviorAttribute { }

/// <summary>
///   Starts the dependency resolution in the direct parent of this injector.
///   This will usually be the module injector if the injector is a component.
/// </summary>
public class SkipSelfAttribute : InjectBehaviorAttribute { }

/// <summary>
///   Restricts the dependency resolution that it can only be resolved by
///   the current module injector. If the dependency is not found
///   within the current module injector, a <see cref="NullInjectorException" /> is thrown.
///   Notes:
///     - SkipSelf and SkipHost do the same in module injectors.
///     - If both Host and SkipHost are specified, SkipHost takes precedence.
///     - If both Self and Host are specified, Host takes precedence.
/// </summary>
public class HostAttribute : InjectBehaviorAttribute { }