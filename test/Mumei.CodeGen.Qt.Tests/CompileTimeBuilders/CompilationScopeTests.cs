using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.RoslynAsExpressionReplacement;
using Mumei.CodeGen.Qt.Tests.Setup;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.CompileTimeBuilders;

public sealed class CompilationScopeTests {
    [Fact]
    public void CompilationScope() {
        var result = new SourceGeneratorTest<CompilationScopeTestSourceGenerator>(b =>
            b.AddReference(SourceCode.Of<CompilationTestSource>())
                .AddTypeReference<CSharpCompilation>()
        ).Run();

        result.PassesAssemblyAction(x => {
            var t = x.GetTypes().First(x => x.Name == nameof(CompilationTestSource));
            var ts = Activator.CreateInstance(t)!;
            var result = ts.GetType().GetMethod(nameof(CompilationTestSource.TestInvocation)).Invoke(ts, []);
            var r = Unsafe.As<CompilationTestSource.TestReceivable>(result);

            Assert.Equal("s", r.ParameterName);
            Assert.Equal("s.Length > 0;", r.Body);
        });
    }
}

file sealed class CompilationTestSource {
    public TestReceivable TestInvocation() {
        var r = new TestReceivable();
        r.Invoke(s => s.Length > 0);
        return r;
    }

    public sealed class TestReceivable : IRoslynExpressionReceivable<Func<string, bool>> {
        public string ParameterName { get; private set; } = null!;
        public string Body { get; private set; } = null!;

        public void Invoke(Func<string, bool> expression) {
            throw new CompileTimeComponentUsedAtRuntimeException();
        }

        public void ReceiveExpression(RoslynExpression<Func<string, bool>> expression) {
            ParameterName = expression.Parameters[0].Identifier.Text;
            Body = expression.Statements[0].NormalizeWhitespace().ToFullString();
        }
    }
}

file sealed class CompilationScopeTestSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterQtSourceOutput(
            Filter,
            GenerateOutput
        );
    }

    private static NodeMatchResult<(InvocationExpressionSyntax Invocation, LambdaExpressionSyntax Lambda)> Filter(NodeMatchingContext ctx) {
        if (ctx.Node is not InvocationExpressionSyntax invocation) {
            return default;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax) {
            return default;
        }

        if (invocation.ArgumentList.Arguments.Count != 1) {
            return default;
        }

        if (invocation.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax lambda) {
            return default;
        }

        return (invocation, lambda);
    }

    private static void GenerateOutput(OutputGenerationContext<(InvocationExpressionSyntax Invocation, LambdaExpressionSyntax Lambda)> ctx) {
        var invocation = ctx.State.Invocation;
        if (ctx.SemanticModel.GetOperation(invocation, ctx) is not IInvocationOperation invocationOperation) {
            return;
        }

        var containingType = invocationOperation.Instance?.Type;
        if (containingType is null) {
            return;
        }

        var implementsExpressionReceivable = containingType.AllInterfaces.Any(x => x.MetadataName == "IRoslynExpressionReceivable`1");
        if (!implementsExpressionReceivable) {
            return;
        }

        var cls = new QtClass(AccessModifier.FileStatic, "cls");
        cls.BindDynamicTemplateInterceptMethod(
            invocation,
            new { ctx.State.Lambda },
            static (ctx, refs) => {
                var px = (LambdaExpressionSyntax) SyntaxFactory.ParseExpression(refs.Lambda.NormalizeWhitespace().ToFullString());
                StatementSyntax[] statements = px.Body is BlockSyntax block
                    ? [..block.Statements]
                    : [SyntaxFactory.ExpressionStatement((ExpressionSyntax) px.Body)];

                ParameterSyntax[] parameters = px is SimpleLambdaExpressionSyntax spx ? [spx.Parameter]
                    : px is ParenthesizedLambdaExpressionSyntax pplx ? [..pplx.ParameterList.Parameters]
                    : [];

                // We can replace the hardcoded Func type once we re-write this to use a method template which can be generic
                var x = new RoslynExpression<Func<string, bool>> {
                    Parameters = parameters,
                    Statements = statements
                };

                ctx.This.Is<IRoslynExpressionReceivable<Func<string, bool>>>().ReceiveExpression(x);
            });

        var ns = QtNamespace.FromGeneratorAssemblyName("Generated", [cls]);
        var file = QtSourceFile.CreateObfuscated("Coolio").WithNamespace(ns);

        ctx.AddFile(file);
    }
}