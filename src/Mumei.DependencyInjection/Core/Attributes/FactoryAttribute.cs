namespace Mumei.Core.Attributes;

/// <summary>
///   Specifies that a method in a module provides the
///   implementation for creating a DI provider instance.
/// </summary>
public class FactoryAttribute : Attribute { }

public class FactoryAttribute<TFactory> : Attribute where TFactory : IDynamicProvider<object> { }

public class ConfigureAttribute : Attribute { }