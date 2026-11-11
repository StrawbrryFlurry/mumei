using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Roslyn.Testing;

[Generator]
public sealed class SourceReferenceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var invocations = context.SyntaxProvider.CreateSyntaxProvider((x, _) => x is InvocationExpressionSyntax {
            Expression: MemberAccessExpressionSyntax {
                Expression: IdentifierNameSyntax { Identifier.Text: nameof(SourceFile) },
                Name: GenericNameSyntax { Identifier.Text: nameof(SourceFile.For) }
            }
        }, (ctx, _) => ctx.Node.ToString());

        context.RegisterSourceOutput(invocations.Collect(), (p, i) => {
            p.AddSource("SourceReferenceInterceptor.g.cs", "");
        });
    }
}