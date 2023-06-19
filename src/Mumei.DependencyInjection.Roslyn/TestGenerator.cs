using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Mumei.DependencyInjection.Roslyn.Module;

namespace Mumei.DependencyInjection.Roslyn;

[Generator]
public class TestGenerator : IIncrementalGenerator {
  private const string MumeiModuleAttribute = "Mumei.DependencyInjection.Attributes.ModuleAttribute";

  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var moduleTypeDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
      static (node, _) => IsTypeDeclarationWithAttribute(node),
      static (context, _) => GetMumeiModuleDeclaration(context)
    );

    var nonNullModuleTypeDeclarations = moduleTypeDeclarations.Where(x => x is not null);

    context.RegisterSourceOutput(nonNullModuleTypeDeclarations,
      (productionContext, syntax) => {
        productionContext.AddSource($"{syntax!.Identifier.Text}Impl.g.cs",
          SourceText.From($"class {syntax.Identifier.Text}Impl {{}}", Encoding.UTF8));
      });
  }

  private static bool IsTypeDeclarationWithAttribute(SyntaxNode node) {
    if (node is not ClassDeclarationSyntax or InterfaceDeclarationSyntax) {
      return false;
    }

    var typeDeclaration = (TypeDeclarationSyntax)node;
    return typeDeclaration.AttributeLists.Count != 0;
  }

  private static TypeDeclarationSyntax? GetMumeiModuleDeclaration(GeneratorSyntaxContext context) {
    var typeDeclaration = (TypeDeclarationSyntax)context.Node;

    var isMumeiModule = typeDeclaration.AttributeLists
      .SelectMany(al => al.Attributes)
      // The symbol is a method symbol, which is the constructor of the attribute.
      .Select(x => context.SemanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)
      .Where(x => x is not null)
      .Select(x => x!.ContainingType)
      .Any(x => x!.ToDisplayString() == MumeiModuleAttribute);

    return isMumeiModule ? typeDeclaration : null;
  }

  private static List<ModuleDeclaration> MakeModuleDeclarationFromTypeDeclaration() {
    return new List<ModuleDeclaration>();
  }
}