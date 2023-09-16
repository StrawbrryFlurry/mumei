using System.Collections.Immutable;
using System.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class ModuleDeclaration {
  public required string Name { get; init; }
  public ModuleDeclaration Parent { get; private set; }
  public required Type DeclaringType { get; init; }

  public required ImmutableArray<ModuleDeclaration> Imports { get; init; }
  public required ImmutableArray<ModuleDeclaration> Exports { get; init; }
  public required ImmutableArray<ComponentDeclaration> Components { get; init; }
  public required ImmutableArray<DynamicProviderBinder> DynamicProviderBinders { get; init; }

  public required ImmutableArray<ProviderSpecification> Providers { get; init; }
  public required ImmutableArray<ModuleProviderConfiguration> ProviderConfigurations { get; init; }

  public required ImmutableArray<PropertyInfo> Properties { get; init; }
  public required ImmutableArray<MethodInfo> Methods { get; init; }

  public void Realize(ModuleDeclaration parent) {
    Parent = parent;
  }
}