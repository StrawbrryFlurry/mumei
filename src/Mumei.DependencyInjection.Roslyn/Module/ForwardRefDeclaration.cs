using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Mumei.DependencyInjection.Injector.Registration;
using Mumei.DependencyInjection.Providers.Registration;
using Mumei.Roslyn;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class ForwardRefDeclaration : IProviderDeclaration {
  public required ForwardRefSource Source { get; init; }
  public required RoslynPropertyInfo Property { get; init; }

  public required RoslynType ProviderType { get; init; }

  public static bool TryCreateFromProperty(
    in RoslynPropertyInfo property,
    in TemporarySpan<RoslynAttribute> attributes,
    [NotNullWhen(true)] out ForwardRefDeclaration? spec
  ) {
    for (var i = 0; i < attributes.Length; i++) {
      var attribute = attributes[i];
      if (!attribute.Is<ForwardRefAttribute>()) {
        continue;
      }

      var forwardRefSource = attribute.GetArgument<ForwardRefSource?>(nameof(ForwardRefAttribute.Source), 0)
                             ?? ForwardRefSource.Default;
      spec = new ForwardRefDeclaration {
        Source = forwardRefSource,
        Property = property,
        ProviderType = property.Type
      };
      return true;
    }

    spec = null;
    return false;
  }
}