using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.DependencyInjection.Roslyn.Module;
using Mumei.Roslyn;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn;

internal ref struct ModuleLoader {
  private readonly RoslynType _rootModule;
  private readonly Compilation _compilation;
  private readonly ReadOnlySpan<CompilationModuleDeclaration> _moduleDeclarations;
  private readonly ReadOnlySpan<CompilationComponentDeclaration> _componentDeclarations;

  private ArrayBuilder<ModuleDeclaration> _globalModules = new();
  private ArrayBuilder<PartialComponentDeclaration> _components = new();

  private ModuleLoader(
    RoslynType rootModule,
    Compilation compilation,
    ReadOnlySpan<CompilationModuleDeclaration> moduleDeclarations,
    ReadOnlySpan<CompilationComponentDeclaration> componentDeclarations
  ) {
    _rootModule = rootModule;
    _compilation = compilation;
    _moduleDeclarations = moduleDeclarations;
    _componentDeclarations = componentDeclarations;
  }

  public static ModuleDeclaration ResolveModule(
    CompilationModuleDeclaration rootModule,
    Compilation compilation,
    ReadOnlySpan<CompilationModuleDeclaration> moduleDeclarations,
    ReadOnlySpan<CompilationComponentDeclaration> componentDeclarations
  ) {
    var rootModuleType = new RoslynType(rootModule.Symbol);
    var loader = new ModuleLoader(rootModuleType, compilation, moduleDeclarations, componentDeclarations);
    // Add global modules to root
    return loader.ResolveModuleRecursively(rootModuleType);
  }

  private ModuleDeclaration ResolveModuleRecursively(RoslynType moduleCompilationType) {
    var isGlobal = false;
    var importsBuilder = new ArrayBuilder<ModuleDeclaration>();
    var dynamicProviderBindersBuilder = new ArrayBuilder<DynamicProviderBinder>();
    var componentsBuilder = new ArrayBuilder<PartialComponentDeclaration>();
    var providersBuilder = new ArrayBuilder<IProviderSpec>();

    using var attributes = moduleCompilationType.GetAttributesTemp();
    for (var i = 0; i < attributes.Length; i++) {
      var attribute = attributes[i];

      // These checks are ordered by frequency of occurrence      
      if (TryGetModuleImport(in attribute, out var importedModule)) {
        var resolvedImport = ResolveModuleRecursively(importedModule.Value);
        importsBuilder.Add(resolvedImport);
        continue;
      }

      if (PartialComponentDeclaration.TryCreateFromAttribute(attribute, out var componentDeclaration)) {
        componentsBuilder.Add(componentDeclaration);
        continue;
      }

      if (DynamicProviderBinder.TryCreateFromAttribute(attribute, out var dynamicProviderBinder)) {
        dynamicProviderBindersBuilder.Add(dynamicProviderBinder);
        continue;
      }

      if (IsGlobalModuleAttribute(attribute)) {
        isGlobal = true;
        continue;
      }

      Debug.Assert(false, "Found an unknown attribute on a module.");
    }

    using var properties = moduleCompilationType.GetPropertiesTemp();
    for (var index = 0; index < properties.Length; index++) {
      var property = properties[index];
      var propertyAttributes = property.GetAttributesTemp();
      if (ProviderSpecification.TryCreateFromProperty(property, propertyAttributes, out var provider)) {
        providersBuilder.Add(provider);
      }

      if (ForwardRefSpecification.TryCreateFromProperty(
            property,
            propertyAttributes,
            out var forwardRefSpecification
          )) {
        providersBuilder.Add(forwardRefSpecification);
      }

      // We don't know what this property is used for, ignore it
    }

    var providerConfigurationsBuilder = new ArrayBuilder<ProviderConfigurationSpec>();
    using var methods = moduleCompilationType.GetMethodsTemp();
    for (var i = 0; i < methods.Length; i++) {
      var method = methods[i];
      var methodAttributes = method.GetAttributesTemp();
      if (FactoryProviderSpecification.TryCreateFromMethod(method, methodAttributes, out var facotryProvider)) {
        providersBuilder.Add(facotryProvider);
        continue;
      }

      if (ProviderConfigurationSpec.TryCreateFromMethod(method, methodAttributes, out var providerConfiguration)) {
        providerConfigurationsBuilder.Add(providerConfiguration);
        continue;
      }

      // We don't know what this method is used for, ignore it
    }

    var imports = importsBuilder.ToImmutableArrayAndFree();
    var module = new ModuleDeclaration {
      Name = moduleCompilationType.Name,
      DeclaringType = moduleCompilationType,
      Imports = imports,
      Components = componentsBuilder.ToImmutableArrayAndFree(),
      DynamicProviderBinders = dynamicProviderBindersBuilder.ToImmutableArrayAndFree(),
      Providers = providersBuilder.ToImmutableArrayAndFree(),
      ProviderConfigurations = providerConfigurationsBuilder.ToImmutableArrayAndFree()
    };

    foreach (var import in imports) {
      import.Realize(module);
    }

    if (isGlobal) {
      _globalModules.Add(module);
    }

    return module;
  }

  private static bool TryGetModuleImport(
    in RoslynAttribute attribute,
    [NotNullWhen(true)] out RoslynType? importedModule
  ) {
    if (attribute.Is<ImportAttribute>()) {
      importedModule = attribute.Type.GetFirstTypeArgument();
      return true;
    }

    if (attribute.IsConstructedGenericTypeOf(typeof(ImportAttribute<>))) {
      importedModule = attribute.Type.GetFirstTypeArgument();
      return true;
    }

    importedModule = null!;
    return false;
  }

  private static bool IsGlobalModuleAttribute(in RoslynAttribute attribute) {
    return attribute.Is<GlobalModuleAttribute>();
  }
}