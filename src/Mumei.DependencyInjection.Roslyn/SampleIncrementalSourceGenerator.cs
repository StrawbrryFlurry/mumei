using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace Mumei.DependencyInjection.Roslyn;

[Generator]
public class SampleIncrementalSourceGenerator : IIncrementalGenerator {
  private const string Namespace = "Generators";
  private const string AttributeName = "ReportAttribute";

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var provider = context.SyntaxProvider
      .CreateSyntaxProvider(
        (s, _) => s is ClassDeclarationSyntax or InterfaceDeclarationSyntax,
        (ctx, _) => GetClassDeclarationForSourceGen(ctx))
      .Where(t => t.isModuleDeclaration)
      .Select((t, _) => t.Item1);

    // Generate the source code.
    context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
      (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
  }

  private static (ClassDeclarationSyntax, bool isModuleDeclaration) GetClassDeclarationForSourceGen(
    GeneratorSyntaxContext context) {
    var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

    foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists) {
      foreach (var attributeSyntax in attributeListSyntax.Attributes) {
        if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol) {
          continue; // if we can't get the symbol, ignore it
        }

        var attributeName = attributeSymbol.ContainingType.ToDisplayString();

        if (attributeName == $"{Namespace}.{AttributeName}") {
          return (classDeclarationSyntax, true);
        }
      }
    }

    return (classDeclarationSyntax, false);
  }

  /// <summary>
  /// Generate code action.
  /// It will be executed on specific nodes (ClassDeclarationSyntax annotated with the [Report] attribute) changed by the user.
  /// </summary>
  /// <param name="context">Source generation context used to add source files.</param>
  /// <param name="compilation">Compilation used to provide access to the Semantic Model.</param>
  /// <param name="classDeclarations">Nodes annotated with the [Report] attribute that trigger the generate action.</param>
  private void GenerateCode(SourceProductionContext context, Compilation compilation,
    ImmutableArray<ClassDeclarationSyntax> classDeclarations) {
    // Go through all filtered class declarations.
    foreach (var classDeclarationSyntax in classDeclarations) {
      // We need to get semantic model of the class to retrieve metadata.
      var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

      // Symbols allow us to get the compile-time information.
      if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol) {
        continue;
      }

      var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

      // 'Identifier' means the token of the node. Get class name from the syntax node.
      var className = classDeclarationSyntax.Identifier.Text;

      // Go through all class members with a particular type (property) to generate method lines.
      var methodBody = classSymbol.GetMembers()
        .OfType<IPropertySymbol>()
        .Select(p =>
          $$"""        yield return $"{{p.Name}}:{this.{{p.Name}}}";"""); // e.g. yield return $"Id:{this.Id}";

      // Build up the source code
      var code = $$"""
                   // <auto-generated/>

                   using System;
                   using System.Collections.Generic;

                   namespace {{namespaceName}};

                   partial class {{className}}
                   {
                       public IEnumerable<string> Report()
                       {
                   {{string.Join("\n", methodBody)}}
                       }
                   }

                   """;

      // Add the source code to the compilation.
      context.AddSource($"{className}.g.cs", SourceText.From(code, Encoding.UTF8));
    }
  }
}