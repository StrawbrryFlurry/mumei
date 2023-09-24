using System.Diagnostics.CodeAnalysis;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class PartialComponentDeclaration {
  public required RoslynType ComponentType { get; init; }

  public static bool TryCreateFromAttribute(
    in RoslynAttribute attribute,
    [NotNullWhen(true)] out PartialComponentDeclaration? component
  ) {
    if (!attribute.IsConstructedGenericTypeOf(typeof(ContributesAttribute<>))) {
      component = null;
      return false;
    }

    component = new PartialComponentDeclaration {
      ComponentType = attribute.Type.GetFirstTypeArgument()
    };
    return true;
  }
}