using System.Diagnostics.CodeAnalysis;
using Mumei.DependencyInjection.Providers.Registration;
using Mumei.Roslyn;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class UseExistingProviderDeclaration : IProviderDeclaration {
  public required RoslynType ProviderType { get; init; }
  public required RoslynType ExistingProviderType { get; init; }

  public static bool TryCreateFromProperty(
    in RoslynPropertyInfo property,
    in TemporarySpan<RoslynAttribute> attributes,
    [NotNullWhen(true)] out UseExistingProviderDeclaration? spec
  ) {
    for (var i = 0; i < attributes.Length; i++) {
      var attribute = attributes[i];
      if (!attribute.IsConstructedGenericTypeOf(typeof(UseExistingAttribute<>))) {
        continue;
      }

      var existingProviderType = attribute.Type.GetFirstTypeArgument();
      spec = new UseExistingProviderDeclaration {
        ProviderType = property.Type,
        ExistingProviderType = existingProviderType
      };
      return true;
    }

    spec = null;
    return false;
  }
}