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
  private readonly CompilationType _rootModule;
  private readonly Compilation _compilation;
  private readonly ReadOnlySpan<CompilationModuleDeclaration> _moduleDeclarations;
  private readonly ReadOnlySpan<CompilationComponentDeclaration> _componentDeclarations;

  private ArrayBuilder<ModuleDeclaration> _globalModules = new();
  private ArrayBuilder<PartialComponentDeclaration> _components = new();

  private ModuleLoader(
    CompilationType rootModule,
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
    var rootModuleType = new CompilationType(rootModule.Symbol);
    var loader = new ModuleLoader(rootModuleType, compilation, moduleDeclarations, componentDeclarations);
    // Add global modules to root
    return loader.ResolveModuleRecursively(rootModuleType);
  }

  private ModuleDeclaration ResolveModuleRecursively(CompilationType moduleCompilationType) {
    var attributes = moduleCompilationType.GetAttributes();

    var importsBuilder = new ArrayBuilder<ModuleDeclaration>();
    var dynamicProviderBindersBuilder = new ArrayBuilder<DynamicProviderBinder>();
    var isGlobal = false;
    var componentsBuilder = new ArrayBuilder<PartialComponentDeclaration>();

    foreach (var attribute in attributes) {
      if (TryGetModuleImport(attribute, out var importedModule)) {
        var resolvedImport = ResolveModuleRecursively(importedModule.Value);
        importsBuilder.Add(resolvedImport);
        continue;
      }

      if (DynamicProviderBinder.TryCreateFromAttribute(attribute, out var dynamicProviderBinder)) {
        dynamicProviderBindersBuilder.Add(dynamicProviderBinder);
        continue;
      }

      if (PartialComponentDeclaration.TryCreateFromAttribute(attribute, out var componentDeclaration)) {
        componentsBuilder.Add(componentDeclaration.Value);
        continue;
      }

      if (IsGlobalModuleAttribute(attribute)) {
        isGlobal = true;
        continue;
      }

      Debug.Assert(false, "Found an unknown attribute on a module.");
    }

    var providersBuilder = new ArrayBuilder<ProviderSpecification>();
    var forwardRefsBuilder = new ArrayBuilder<ForwardRefSpecification>();

    List<PropertyInfo> properties = null!; // moduleCompilationType.GetProperties();
    foreach (var property in properties) {
      if (ProviderSpecification.TryCreateFromProperty(property, out var provider)) {
        providersBuilder.Add(provider);
      }

      if (ForwardRefSpecification.TryCreateFromProperty(property, out var forwardRefSpecification)) {
        forwardRefsBuilder.Add(forwardRefSpecification);
      }

      // We don't know what this property is used for, ignore it
    }

    var providerConfigurationsBuilder = new ArrayBuilder<ModuleProviderConfiguration>();
    var factoryProvidersBuilder = new ArrayBuilder<FactoryProviderSpecification>();
    List<MethodInfo> methods = null; // moduleCompilationType.GetMethods();
    foreach (var method in methods) {
      if (ModuleProviderConfiguration.TryCreateFromMethod(method, out var providerConfiguration)) {
        providerConfigurationsBuilder.Add(providerConfiguration);
      }

      if (FactoryProviderSpecification.TryCreateFromMethod(method, out var facotryProvider)) {
        factoryProvidersBuilder.Add(facotryProvider);
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
      ProviderConfigurations = providerConfigurationsBuilder.ToImmutableArrayAndFree(),
      Properties = properties.ToImmutableArray(),
      Methods = methods.ToImmutableArray(),
      FactoryProviders = factoryProvidersBuilder.ToImmutableArrayAndFree()
    };

    foreach (var import in imports) {
      import.Realize(module);
    }

    if (isGlobal) {
      _globalModules.Add(module);
    }

    return module;
  }

  private static bool TryGetModuleImport(CompilationAttribute attribute,
    [NotNullWhen(true)] out CompilationType? importedModule) {
    if (attribute.Is<ImportAttribute>()) {
      importedModule = attribute.GetArgument<CompilationType>();
      return true;
    }

    if (attribute.IsConstructedGenericTypeOf(typeof(ImportAttribute<>))) {
      importedModule = attribute.Type.GetFirstTypeArgument();
      return true;
    }

    importedModule = null!;
    return false;
  }

  private static bool IsGlobalModuleAttribute(CompilationAttribute attribute) {
    return attribute is GlobalModuleAttribute;
  }
}