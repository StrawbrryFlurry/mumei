using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

/// <summary>
/// A provider configuration within a module
/// <code>
/// interface ISomeModule {
///   => public IHttpClient ConfigureHttpClient(IHttpClient client) { ... };
/// }
/// </code>
/// </summary>
internal class ModuleProviderConfiguration {
  public ModuleDeclaration Declaration { get; set; }
  public Type ProviderType { get; set; }

  public List<Type> TargetProviders { get; set; } = new();
  public bool ShouldApplyToAll { get; set; }
  public MethodInfo ConfigurationMethod { get; set; }

  public static bool TryCreateFromMethod(
    MethodInfo method,
    [NotNullWhen(true)] out ModuleProviderConfiguration? providerConfiguration
  ) {
    throw new NotImplementedException();
  }
}