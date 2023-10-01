using System.Diagnostics.CodeAnalysis;
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

  private ArrayBuilder<ModuleDeclaration> _globalModules = new();
  private ArrayBuilder<PartialComponentDeclaration> _components = new();

  private ModuleLoader(
    RoslynType rootModule,
    Compilation compilation
  ) {
    _rootModule = rootModule;
    _compilation = compilation;
  }

  public static ModuleDeclaration ResolveRootModule(
    ITypeSymbol rootModule,
    Compilation compilation
  ) {
    var rootModuleType = new RoslynType(rootModule);
    var loader = new ModuleLoader(rootModuleType, compilation);
    // Add global modules to root
    return loader.ResolveModuleRecursively(rootModuleType);
  }

  private ModuleDeclaration ResolveModuleRecursively(RoslynType moduleCompilationType) {
    var isGlobal = false;
    var importsBuilder = new ArrayBuilder<ModuleDeclaration>();
    var dynamicProviderBindersBuilder = new ArrayBuilder<DynamicProviderBinder>();
    var componentsBuilder = new ArrayBuilder<PartialComponentDeclaration>();
    var providersBuilder = new ArrayBuilder<IProviderDeclaration>();

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
        _components.Add(componentDeclaration);
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
    }

    using var properties = moduleCompilationType.GetPropertiesTemp();
    for (var i = 0; i < properties.Length; i++) {
      var property = properties[i];
      var propertyAttributes = property.GetAttributesTemp();
      if (ProviderDeclaration.TryCreateFromProperty(property, propertyAttributes, out var provider)) {
        providersBuilder.Add(provider);
      }

      if (UseExistingProviderDeclaration.TryCreateFromProperty(property, attributes, out var useExistingProvider)) {
        providersBuilder.Add(useExistingProvider);
      }

      if (ForwardRefDeclaration.TryCreateFromProperty(property, propertyAttributes, out var forwardRefProvider)) {
        providersBuilder.Add(forwardRefProvider);
      }

      // We don't know what this property is used for, ignore it
    }

    var providerConfigurationsBuilder = new ArrayBuilder<ProviderConfigurationDeclaration>();
    using var methods = moduleCompilationType.GetMethodsTemp();
    for (var i = 0; i < methods.Length; i++) {
      var method = methods[i];
      var methodAttributes = method.GetAttributesTemp();
      if (FactoryProviderDeclaration.TryCreateFromMethod(method, methodAttributes, out var facotryProvider)) {
        providersBuilder.Add(facotryProvider);
        continue;
      }

      if (ProviderConfigurationDeclaration.TryCreateFromMethod(method, methodAttributes,
            out var providerConfiguration)) {
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