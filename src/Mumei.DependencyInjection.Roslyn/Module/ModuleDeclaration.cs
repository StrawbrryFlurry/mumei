using System.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module; 

internal class ModuleDeclaration {
  public string Name { get; set; }
  public Type InterfaceDeclaration { get; set; }

  public List<ModuleDeclaration> Imports { get; set; }
  public List<ModuleDeclaration> Exports { get; set; }
  public List<ComponentDeclaration> Components { get; set; }

  public List<ProviderSpecification> Providers { get; set; }
  public List<ModuleProviderConfiguration> ProviderConfigurations { get; set; }

  public List<PropertyInfo> Properties { get; set; }
}