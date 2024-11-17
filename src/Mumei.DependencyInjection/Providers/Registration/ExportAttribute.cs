namespace Mumei.DependencyInjection.Providers.Registration;

/// <summary>
/// Marks the decorated provider as an export,
/// allowing it to be used by modules referencing the containing module
/// </summary>
public sealed class ExportAttribute : Attribute;