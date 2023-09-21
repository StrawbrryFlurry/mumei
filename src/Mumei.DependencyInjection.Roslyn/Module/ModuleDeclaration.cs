using System.Collections.Immutable;
using System.Reflection;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class ModuleDeclaration {
  public required string Name { get; init; }
  public ModuleDeclaration Parent { get; private set; }
  public required RoslynType DeclaringType { get; init; }

  public required ImmutableArray<ModuleDeclaration> Imports { get; init; }
  public required ImmutableArray<PartialComponentDeclaration> Components { get; init; }
  public required ImmutableArray<DynamicProviderBinder> DynamicProviderBinders { get; init; }

  public required ImmutableArray<ProviderSpecification> Providers { get; init; }
  public required ImmutableArray<FactoryProviderSpecification> FactoryProviders { get; init; }
  public required ImmutableArray<ModuleProviderConfiguration> ProviderConfigurations { get; init; }

  public void Realize(ModuleDeclaration parent) {
    Parent = parent;
  }
}