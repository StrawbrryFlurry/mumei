using System.Diagnostics.CodeAnalysis;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class ComponentDeclaration {
  public string Name { get; } = null!;
  public List<ProviderSpecification> Providers { get; } = null!;

  public static bool TryGetComponentDeclaration(
    Attribute attribute,
    [NotNullWhen(true)] out ComponentDeclaration? componentDeclaration
  ) {
    componentDeclaration = null;
    return true;
  }
}