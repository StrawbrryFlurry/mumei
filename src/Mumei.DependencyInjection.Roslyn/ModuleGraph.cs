using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Roslyn.Module;
using Mumei.Roslyn;
using Mumei.Roslyn.Extensions;

namespace Mumei.DependencyInjection.Roslyn;

internal sealed class ModuleGraph {
    public ModuleDeclaration Root { get; init; } = null!;

    public static ReadOnlySpan<ModuleGraph> CreateFromCompilation(
        Compilation compilation,
        ReadOnlySpan<CompilationModuleDeclaration> moduleDeclarations,
        ReadOnlySpan<CompilationComponentDeclaration> componentDeclarations
    ) {
        var entrypoints = GetEntrypointDeclarations(compilation, moduleDeclarations);
        var realizedEntrypoints = new ModuleDeclaration[entrypoints.Length];

        for (var i = 0; i < entrypoints.Length; i++) {
            var sm = compilation.GetSemanticModel(entrypoints[i].Syntax.SyntaxTree);
            var entrypoint = sm.GetDeclaredSymbol(entrypoints[i].Syntax)!;
            realizedEntrypoints[i] = ResolveEntrypointModule(
                compilation,
                entrypoint,
                moduleDeclarations,
                componentDeclarations
            );
        }

        return new List<ModuleGraph>().ToImmutableArray().AsSpan();
    }

    private static ModuleDeclaration ResolveEntrypointModule(
        Compilation compilation,
        INamedTypeSymbol rootModuleDeclaration,
        ReadOnlySpan<CompilationModuleDeclaration> moduleDeclarations,
        ReadOnlySpan<CompilationComponentDeclaration> componentDeclarations
    ) {
        return ModuleLoader.ResolveRootModule(
            rootModuleDeclaration,
            compilation
        );
    }


    private static ImmutableArray<CompilationModuleDeclaration> GetEntrypointDeclarations(
        Compilation compilation,
        ReadOnlySpan<CompilationModuleDeclaration> moduleDeclarations
    ) {
        var rootModules = new ArrayBuilder<CompilationModuleDeclaration>();
        foreach (var module in moduleDeclarations) {
            var sm = compilation.GetSemanticModel(module.Syntax.SyntaxTree);
            if (!module.Syntax.TryGetAttribute(sm, RootModuleAttribute.FullName, out _)) {
                continue;
            }

            rootModules.Add(module);
        }

        return rootModules.ToImmutableArrayAndFree();
    }
}