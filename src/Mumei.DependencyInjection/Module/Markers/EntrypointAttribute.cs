namespace Mumei.DependencyInjection.Module.Markers;

/// <summary>
///   Marks a module as an application root. Mumei will generate an
///   Application Environment for all application roots.
/// </summary>
public sealed class EntrypointAttribute : Attribute {
  public static readonly string FullName = typeof(EntrypointAttribute).FullName!;
}