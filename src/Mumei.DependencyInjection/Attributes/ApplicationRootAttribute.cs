namespace Mumei.DependencyInjection.Attributes;

/// <summary>
///   Marks a module as an application root. Mumei will generate an
///   Application Environment for all application roots.
/// </summary>
public sealed class ApplicationRootAttribute : Attribute { }