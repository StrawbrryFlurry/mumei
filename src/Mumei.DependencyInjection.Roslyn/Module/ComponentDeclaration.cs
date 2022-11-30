namespace Mumei.DependencyInjection.Roslyn.Module; 

internal sealed class ComponentDeclaration {
  public string Name { get; }
  public List<ProviderSpecification> Providers { get; }
}