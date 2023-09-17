namespace Mumei.DependencyInjection.Module.Markers;

/// <summary>
///   Declares the interface as the root module for an application environment.
///   This attribute can be omitted, if the root module can be determined statically,
///   for example by calling PlatformInjector.CreateEnvironment{TRootModule} in the application startup.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class RootModuleAttribute : Attribute {
  public static readonly string FullName = typeof(RootModuleAttribute).FullName!;
}