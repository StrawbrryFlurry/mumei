using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.RoslynAsExpressionReplacement;
using Mumei.Roslyn.Testing;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.CompileTimeBuilders;

public sealed class InterceptorMethodTemplateTests {
    [Fact]
    public void TemplateMethod() {
        var result = new SourceGeneratorTest<CompilationScopeTestSourceGenerator>(b =>
            b.AddReference(SourceCode.Of<CompilationTestSource>())
                .AddTypeReference<CSharpCompilation>()
        ).Run();

        result.PassesAssemblyAction(x => {
            var t = x.CreateInstance<CompilationTestSource>();
            var r = t.Invoke(ts => ts.TestInvocation());

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

public sealed class RoslynExpressionReceivableTemplate<TExpression>(LambdaExpressionSyntax lambda) : QtInterceptorMethodTemplate, IRoslynExpressionReceivable<TExpression>
    where TExpression : Delegate {
    public void Invoke(TExpression expression) {
        var px = (LambdaExpressionSyntax) SyntaxFactory.ParseExpression(lambda.NormalizeWhitespace().ToFullString());
        StatementSyntax[] statements = px.Body is BlockSyntax block
            ? [..block.Statements]
            : [SyntaxFactory.ExpressionStatement((ExpressionSyntax) px.Body)];

        ParameterSyntax[] parameters = px is SimpleLambdaExpressionSyntax spx ? [spx.Parameter]
            : px is ParenthesizedLambdaExpressionSyntax pplx ? [..pplx.ParameterList.Parameters]
            : [];

        var x = new RoslynExpression<TExpression> {
            Parameters = parameters,
            Statements = statements
        };

        ReceiveExpression(x);
    }

    public void ReceiveExpression(RoslynExpression<TExpression> expression) {
        throw new NotSupportedException();
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
            return NodeMatchResult.None;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax) {
            return NodeMatchResult.None;
        }

        if (invocation.ArgumentList.Arguments.Count != 1) {
            return NodeMatchResult.None;
        }

        if (invocation.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax lambda) {
            return NodeMatchResult.None;
        }

        return NodeMatchResult.Of((invocation, lambda));
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
        cls.AddTemplateInterceptMethod(
            invocation,
            new RoslynExpressionReceivableTemplate<Delegate>(ctx.State.Lambda),
            t => t.Invoke
        );

        var ns = QtNamespace.FromGeneratorAssemblyName("Generated", [cls]);
        var file = QtSourceFile.CreateObfuscated("Coolio").WithNamespace(ns);

        ctx.AddFile(file);
    }
}

file static class FieldReflection {
#if NET7_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<lambda>P")]
    public static extern LambdaExpressionSyntax RoslynExpressionReceivableTemplate_StateArg_1(RoslynExpressionReceivableTemplate<Delegate> instance);
#else
    public static LambdaExpressionSyntax RoslynExpressionReceivableTemplate_StateArg_1(RoslynExpressionReceivableTemplate<Delegate> instance) {
        var f = typeof(RoslynExpressionReceivableTemplate<Delegate>).GetField("<lambda>P", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (LambdaExpressionSyntax) f!.GetValue(instance)!;
    }
#endif
}