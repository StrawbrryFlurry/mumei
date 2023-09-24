using System.Collections.Immutable;
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
internal sealed class ProviderConfigurationDeclaration {
  public required RoslynType ProviderType { get; init; }
  public required RoslynMethodInfo ConfigurationMethod { get; init; }

  public required ImmutableArray<RoslynType> TypesToConfigure { get; init; }

  public required bool ShouldApplyToAll { get; set; }

  public static bool TryCreateFromMethod(
    in RoslynMethodInfo method,
    in TemporarySpan<RoslynAttribute> attributes,
    [NotNullWhen(true)] out ProviderConfigurationDeclaration? providerConfiguration
  ) {
    var applyToAll = false;
    var typesToConfigure = new ArrayBuilder<RoslynType>();
    for (var i = 0; i < attributes.Length; i++) {
      var attribute = attributes[i];
      if (attribute.IsConstructedGenericTypeOf(typeof(ConfigureForAttribute<>))) {
        typesToConfigure.Add(attribute.Type.GetFirstTypeArgument());
      }

      if (attribute.Is<ConfigureAttribute>()) {
        applyToAll = true;
      }
    }

    providerConfiguration = new ProviderConfigurationDeclaration {
      ConfigurationMethod = method,
      ProviderType = method.ReturnType,
      ShouldApplyToAll = applyToAll,
      TypesToConfigure = typesToConfigure.ToImmutableArrayAndFree()
    };
    return true;
  }
}