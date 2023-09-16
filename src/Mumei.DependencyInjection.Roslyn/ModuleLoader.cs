using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.DependencyInjection.Roslyn.Module;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn;

internal ref struct ModuleLoader {
  private readonly Type _rootModule;
  private readonly Compilation _compilation;
  private readonly ReadOnlySpan<CompilationModuleDeclaration> _moduleDeclarations;
  private readonly ReadOnlySpan<CompilationComponentDeclaration> _componentDeclarations;

  private ArrayBuilder<ModuleDeclaration> _globalModules = new();
  private ArrayBuilder<PartialComponentDeclaration> _components = new();

  private ModuleLoader(
    Type rootModule,
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
    var rootModuleType = rootModule.Symbol.ToType();
    var loader = new ModuleLoader(rootModuleType, compilation, moduleDeclarations, componentDeclarations);
    // Add global modules to root
    return loader.ResolveModuleRecursively(rootModuleType);
  }

  private ModuleDeclaration ResolveModuleRecursively(Type moduleType) {
    var attributes = moduleType.GetCustomAttributes();
    var importsBuilder = new ArrayBuilder<ModuleDeclaration>();
    var dynamicProviderBindersBuilder = new ArrayBuilder<DynamicProviderBinder>();
    var isGlobal = false;

    foreach (var attribute in attributes) {
      if (TryGetModuleImport(attribute, out var importedModule)) {
        var resolvedImport = ResolveModuleRecursively(importedModule);
        importsBuilder.Add(resolvedImport);
        continue;
      }

      if (TryGetDynamicProviderBinder(attribute, out var dynamicProviderBinder)) {
        dynamicProviderBindersBuilder.Add(dynamicProviderBinder);
        continue;
      }

      if (IsGlobalModuleAttribute(attribute)) {
        isGlobal = true;
        continue;
      }

      Debug.Assert(false, "Found an unknown attribute on a module.");
    }

    var imports = importsBuilder.ToImmutableArrayAndFree();
    var module = new ModuleDeclaration {
      Name = moduleType.Name,
      DeclaringType = moduleType,
      Imports = imports,
      Exports = default,
      Components = default,
      DynamicProviderBinders = default,
      Providers = default,
      ProviderConfigurations = default,
      Properties = default,
      Methods = default
    };

    foreach (var import in imports) {
      import.Realize(module);
    }

    if (isGlobal) {
      _globalModules.Add(module);
    }
      
    return module;
  }

  private static bool TryGetModuleImport(Attribute attribute, [NotNullWhen(true)] out Type? importedModule) {
    if (attribute is ImportAttribute importAttribute) {
      importedModule = importAttribute.Module;
      return true;
    }

    var attributeType = attribute.GetType();
    if (attributeType.GetGenericTypeDefinition() == typeof(ImportAttribute<>)) {
      importedModule = attributeType.GetGenericArguments()[0];
      return true;
    }

    importedModule = null!;
    return false;
  }

  private static bool IsGlobalModuleAttribute(Attribute attribute) {
    return attribute is GlobalModuleAttribute;
  }

  private static bool TryGetDynamicProviderBinder(
    Attribute attribute,
    [NotNullWhen(true)] out Type? dynamicProviderBinder
  ) {
    var attributeType = attribute.GetType();
    if (attributeType.GetGenericTypeDefinition() == typeof(DynamicallyBindAttribute<>)) {
      dynamicProviderBinder = attributeType.GetGenericArguments()[0];
      return true;
    }

    dynamicProviderBinder = null!;
    return false;
  }
}