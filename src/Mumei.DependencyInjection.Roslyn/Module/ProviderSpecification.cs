using System.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module; 

/// <summary>
/// A single provider specified in a module.
/// <code>
/// interface ISomeModule {
///   [Singleton{SomeImplementation}]
///   public ISomeProvider SomeProvider { get; }
/// }
/// </code>
/// </summary>
internal class ProviderSpecification {
  public ModuleDeclaration Declaration { get; set; }
  public PropertyInfo DeclarationProperty { get; set; }
  public Type ProviderType { get; set; }
  public string ProviderName { get; set; }
  public object ProviderToken { get; set; }
  public Type ImplementationType { get; set; }

  public int ProviderLifetime { get; set; }
}