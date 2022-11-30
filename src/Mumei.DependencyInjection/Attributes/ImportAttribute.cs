namespace Mumei.Attributes;

/// <summary>
///   Imports the module specified by the decorated property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ImportAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Interface)]
public sealed class ImportAttribute<TModule> : DependencyRegistrationAttribute<TModule> { }