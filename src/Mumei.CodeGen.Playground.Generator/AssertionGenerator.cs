using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Playground.Generator;

[Generator]
public class AssertionGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var refs = context.SyntaxProvider.CreateSyntaxProvider((s, _) =>
            s is InvocationExpressionSyntax {
                Expression: MemberAccessExpressionSyntax {
                    Name.Identifier.Text: "Assert"
                }
            }, (x, _) => true);

        context.RegisterSourceOutput(refs, (s, c) => {
            s.AddSource("foo.Assert.g.cs", """
                                           file static class Assert {}
                                           """);
        });
    }
}