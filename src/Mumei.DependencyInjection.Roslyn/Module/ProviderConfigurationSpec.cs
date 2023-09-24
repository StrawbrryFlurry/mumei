using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Mumei.DependencyInjection.Providers.Registration;
using Mumei.Roslyn;
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
internal sealed class ProviderConfigurationSpec {
  public Type ProviderType { get; set; }
  public bool ShouldApplyToAll { get; set; }

  private RoslynMethodInfo _configurationMethod;
  public ref RoslynMethodInfo ConfigurationMethod => ref _configurationMethod;

  public static bool TryCreateFromMethod(
    in RoslynMethodInfo method,
    in TemporarySpan<RoslynAttribute> attributes,
    [NotNullWhen(true)] out ProviderConfigurationSpec? providerConfiguration
  ) {
    for (var i = 0; i < attributes.Length; i++) {
      var attribute = attributes[i];
      if (attribute.IsConstructedGenericTypeOf(typeof(ConfigureForAttribute<>))) { }

      if (attribute.Is<ConfigureAttribute>()) { }
    }

    providerConfiguration = new ProviderConfigurationSpec {
      ConfigurationMethod = method
    };
    return true;
  }
}