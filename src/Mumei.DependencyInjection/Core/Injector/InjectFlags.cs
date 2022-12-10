namespace Mumei.DependencyInjection.Core;

[Flags]
public enum InjectFlags {
  None = 1 << 0,
  Optional = 1 << 2,
  Host = 1 << 5,
  SkipHost = 1 << 6,
  Lazy = 1 << 7
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
/// </summary>
public class HostAttribute : InjectBehaviorAttribute { }

/// <summary>
///   Starts dependency resolution in the parent of the current module injector.
/// </summary>
public class SkipHostAttribute : InjectBehaviorAttribute { }