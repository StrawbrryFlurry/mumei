using Mumei.Core.Injector;

namespace Mumei.Core;

[Flags]
public enum InjectFlags {
  None = 1 << 0,
  Optional = 1 << 2,
  Host = 1 << 5,
  SkipHost = 1 << 6,
  Lazy = 1 << 7
}

/// <summary>
///   Marks the dependency as optional and injects null if the dependency is not found.
/// </summary>
public class OptionalAttribute : Attribute { }

/// <summary>
///   Restricts the dependency resolution to the current injector. If the dependency is
///   not found in in this injector a <see cref="NullInjectorException" /> is thrown.
/// </summary>
public class SelfAttribute : Attribute { }

/// <summary>
///   Starts the dependency resolution in the direct parent of this injector.
///   This will usually be the module injector if the injector is a component.
/// </summary>
public class SkipSelfAttribute : Attribute { }

/// <summary>
///   Restricts the dependency resolution that it can only be resolved by
///   the current module injector. If the dependency is not found
///   within the current module injector, a <see cref="NullInjectorException" /> is thrown.
/// </summary>
public class HostAttribute : Attribute { }

/// <summary>
///   Starts dependency resolution in the parent of the current module injector.
/// </summary>
public class SkipHostAttribute : Attribute { }