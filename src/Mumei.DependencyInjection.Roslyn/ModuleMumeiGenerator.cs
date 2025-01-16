using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.Roslyn.Extensions;
using ClassDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;

namespace Mumei.DependencyInjection.Roslyn;

internal struct CompilationModuleDeclaration {
    public required TypeDeclarationSyntax Syntax { get; init; }
}

internal struct CompilationComponentDeclaration {
    public required TypeDeclarationSyntax Syntax { get; init; }
}

[Generator]
public class ModuleMumeiGenerator : IIncrementalGenerator {
    private static readonly string MumeiModuleAttributeName = typeof(ModuleAttribute).FullName!;
    private static readonly string MumeiComponentAttributeName = typeof(ComponentAttribute).FullName!;
    private static readonly string MumeiEntrypointAttributeName = typeof(RootModuleAttribute).FullName!;

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var moduleDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MumeiModuleAttributeName,
                (node, _) => node is ClassDeclarationSyntax or InterfaceDeclarationSyntax,
                (syntaxContext, _) => new CompilationModuleDeclaration {
                    Syntax = (TypeDeclarationSyntax)syntaxContext.TargetNode
                }
            );

        var componentDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MumeiComponentAttributeName,
                (node, _) => node is PropertyDeclarationSyntax,
                (syntaxContext, _) => new CompilationComponentDeclaration {
                    Syntax = (TypeDeclarationSyntax)syntaxContext.TargetNode.Parent!
                }
            );

        var declarations = moduleDeclarations.Collect()
            .Combine(componentDeclarations.Collect());

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(
                declarations
            ),
            (ctx, t) => GenerateCode(ctx, t.Left, t.Right.Left, t.Right.Right)
        );
    }

    private static void GenerateCode(
        SourceProductionContext ctx,
        Compilation compilation,
        ImmutableArray<CompilationModuleDeclaration> moduleDeclarations,
        ImmutableArray<CompilationComponentDeclaration> componentDeclarations
    ) {
        var entrypointModules = moduleDeclarations
            .Where(module =>
                module.Syntax.TryGetAttribute(
                    compilation.GetSemanticModel(module.Syntax.SyntaxTree),
                    MumeiEntrypointAttributeName, out _
                )
            ).ToList();

        foreach (var module in moduleDeclarations) {
            if (!module.Syntax.IsPartial()) {
                ctx.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "ModuleDeclarationNeedsTobePartial",
                            "ModuleDeclarationNeedsTobePartial",
                            "ModuleDeclarationNeedsTobePartial",
                            "ModuleDeclarationNeedsTobePartial",
                            DiagnosticSeverity.Error,
                            true
                        ),
                        module.Syntax.GetLocation()
                    )
                );
            }

            var keyword = module.Syntax switch {
                ClassDeclarationSyntax => "class",
                InterfaceDeclarationSyntax => "interface",
                _ => throw new NotSupportedException()
            };

            var sm = compilation.GetSemanticModel(module.Syntax.SyntaxTree);
            var moduleSymbol = sm.GetDeclaredSymbol(module.Syntax)!;
            ctx.AddSource(
                $"{moduleSymbol.Name}.ModuleImpl.g.cs",
                $$"""
                  public sealed class λ{{moduleSymbol.Name}} : {{moduleSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} {

                  }
                  """
            );
        }
    }
}