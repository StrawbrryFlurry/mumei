using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

/// <summary>
/// A provider configuration within a module
/// <code>
/// interface ISomeModule {
///   => public IHttpClient ConfigureHttpClient(IHttpClient client) { ... };
/// }
/// </code>
/// </summary>
internal sealed class ModuleProviderConfiguration {
  public ModuleDeclaration Declaration { get; set; }
  public Type ProviderType { get; set; }

  public List<Type> TargetProviders { get; set; } = new();
  public bool ShouldApplyToAll { get; set; }

  private RoslynMethodInfo _configurationMethod;
  public ref RoslynMethodInfo ConfigurationMethod => ref _configurationMethod;

  public static bool TryCreateFromMethod(
    in RoslynMethodInfo method,
    [NotNullWhen(true)] out ModuleProviderConfiguration? providerConfiguration
  ) {
    providerConfiguration = new ModuleProviderConfiguration {
      ConfigurationMethod = method
    };
    return true;
  }
}