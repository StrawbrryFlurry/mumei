namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class ComponentDeclaration {
  public string Name { get; } = null!;
  public List<ProviderSpecification> Providers { get; } = null!;
}