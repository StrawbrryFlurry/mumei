using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.SyntaxNodes;
using BlockSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax;
using ExpressionStatementSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax;

namespace Mumei.Roslyn.SourceGeneration;

[Generator]
public class TestSourceGenerator : ISourceGenerator {
  private readonly ClassSyntaxBuilderReceiver _receiver = new();

  public void Initialize(GeneratorInitializationContext context) {
    context.RegisterForSyntaxNotifications(() => _receiver);
  }

  public void Execute(GeneratorExecutionContext context) {
    var compilation = context.Compilation;

    foreach (var createClassBuilderExpression in _receiver.CreateClassBuilderExpressions) {
      var semanticModel = compilation.GetSemanticModel(createClassBuilderExpression.SyntaxTree);
      var classBuilderType = (INamedTypeSymbol)semanticModel.GetTypeInfo(createClassBuilderExpression).Type!;
      var classArgument = classBuilderType.TypeArguments[0];
      var source = $@"public class {classArgument.Name} {{
}}";

      context.AddSource($"{classArgument.Name}ClassTemplate.g.cs", source);

      if (createClassBuilderExpression.Parent?.Parent is not VariableDeclaratorSyntax variableDeclarator) {
        continue;
      }

      TrackVariableUsageInLocalContext(compilation, variableDeclarator);
    }
  }
  
  private void TrackVariableUsageInLocalContext(Compilation compilation, VariableDeclaratorSyntax variableDeclarator) {
      var variableDeclaration = (VariableDeclarationSyntax)variableDeclarator.Parent!;
    var declarationStatement = (LocalDeclarationStatementSyntax)variableDeclaration.Parent!;
    var block = (BlockSyntax)declarationStatement.Parent!;
    
    var statementsAfterVariableDeclaration = block.Statements
      .SkipWhile(s => s != declarationStatement)
      .Skip(1)
      .ToArray();
    
    var reassignment = statementsAfterVariableDeclaration
      .FirstOrDefault(s => s.DescendantNodes()
        .OfType<AssignmentExpressionSyntax>()
        .Any(eq => eq.DescendantNodes()
          .OfType<IdentifierNameSyntax>()
          .Any(i => i.Identifier.ValueText == variableDeclarator.Identifier.ValueText)));

    var relevantStatements = statementsAfterVariableDeclaration.TakeWhile(s => s != reassignment);
    
    // relevantStatements.Where(x => x.)
    
    var invocationsOnVariable = relevantStatements
      .OfType<ExpressionStatementSyntax>()
      .Select(s => s.Expression)
      .OfType<InvocationExpressionSyntax>()
      .Where(i => i.Expression is MemberAccessExpressionSyntax memberAccess
                  && memberAccess.Expression is IdentifierNameSyntax identifier
                  && identifier.Identifier.ValueText == variableDeclarator.Identifier.ValueText);
    
    var semanticModel = compilation.GetSemanticModel(variableDeclarator.SyntaxTree);
    foreach (var invocation in invocationsOnVariable) {
      var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation).Symbol!;
      
      var methodArgumentTypes = methodSymbol.Parameters
        .Select(p => p.Type)
        .Select(t => t.ToDisplayString())
        .ToArray();
      
      var delegateBuilder = new StringBuilder();
      var methodArgument = methodSymbol.TypeArguments[0];
    }
  }

  private void CreateDelegateType(Compilation compilation, IMethodSymbol methodSymbol) {

  }
  
}

internal sealed class ClassSyntaxBuilderReceiver : ISyntaxContextReceiver {
  public List<ObjectCreationExpressionSyntax> CreateClassBuilderExpressions { get; } = new();

  public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
    if (context.Node is not ObjectCreationExpressionSyntax objectCreationExpressionSyntax) {
      return;
    }

    if (objectCreationExpressionSyntax.Type is not GenericNameSyntax nameSyntax) {
      return;
    }

    var isClassBuilderType = nameSyntax.Identifier.Text == "ClassSyntaxBuilder";
    if (isClassBuilderType) {
      CreateClassBuilderExpressions.Add(objectCreationExpressionSyntax);
    }
  }
}