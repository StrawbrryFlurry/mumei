using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.RoslynAsExpressionReplacement;
using Mumei.CodeGen.Qt.Tests.Setup;
using Mumei.CodeGen.Qt.TwoStageBuilders.Components;
using Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.Testing;

public sealed class SyntheticClassMethodDeclarationGeneratorTests_Usage {
    [Fact]
    public void Test() {
        var result = new SourceGeneratorTest<TestGenerator>(b =>
            b.AddReference(SourceCode.Of<CompilationTestSource>()).AddTypeReference<CSharpCompilation>().WithAssemblyName("TestAssembly")
        ).Run();
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

        public T Get<T>(T param) where T : Delegate {
            return param;
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

file sealed class TestGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var invocations = context.SyntaxProvider.CreateSyntaxProvider(
            Filter,
            GenerateOutput
        );

        context.RegisterSourceOutput(invocations, (productionContext, node) => {
            var compilation = new SyntheticCompilation(node.SemanticModel.Compilation);
            var cls = compilation.DeclareClass("Test")
                .WithModifiers(AccessModifierList.File + AccessModifierList.Static);

            cls.DeclareInterceptorMethod(
                cls.MakeUniqueName("Intercept_Invoke"),
                node.Invocation
            ).WithBody(new { Lambda = node.State }, static (state, ctx) => {
                var px = (LambdaExpressionSyntax) SyntaxFactory.ParseExpression(state.Lambda.NormalizeWhitespace().ToFullString());
                StatementSyntax[] statements = px.Body is BlockSyntax block
                    ? [..block.Statements]
                    : [SyntaxFactory.ExpressionStatement((ExpressionSyntax) px.Body)];

                ParameterSyntax[] parameters = px is SimpleLambdaExpressionSyntax spx ? [spx.Parameter]
                    : px is ParenthesizedLambdaExpressionSyntax pplx ? [..pplx.ParameterList.Parameters]
                    : [];

                var x = new RoslynExpression<Delegate> {
                    Parameters = parameters,
                    Statements = statements
                };

                ctx.This<IRoslynExpressionReceivable<Delegate>>().ReceiveExpression(x);
            });

            var ns = compilation.NamespaceFromCompilation("Generated").WithMember(cls);
            var fragment = compilation.Synthesize<NamespaceFragment>(ns);
            var file = new SourceFileRenderTreeBuilder();
            file.RenderRootNode(fragment);
            productionContext.AddSource("Generated_Test.g.cs", file.GetSourceText());
        });
    }

    private static bool Filter(SyntaxNode node, CancellationToken ct) {
        if (!SyntaxNodeFilter.IsInvocationOf(node, "Invoke", out var invocation)) {
            return false;
        }

        if (invocation.ArgumentList.Arguments.Count != 1) {
            return false;
        }

        if (invocation.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax lambda) {
            return false;
        }

        return true;
    }

    private static InterceptInvocationIntermediateNode<LambdaExpressionSyntax> GenerateOutput(GeneratorSyntaxContext ctx, CancellationToken ct) {
        if (!SyntaxProviderFactory.TryCreateInvocation(ctx, out var node)) {
            return IntermediateNode.None;
        }

        if (!node.MethodInfo.IsImplementedFromAnyConstructedFormOf(typeof(IRoslynExpressionReceivable<>))) {
            return IntermediateNode.None;
        }

        // var cls = new QtClass(AccessModifier.FileStatic, "cls");
        // cls.AddTemplateInterceptMethod(
        //     invocation,
        //     new RoslynExpressionReceivableTemplate<Delegate>(ctx.State.Lambda),
        //     t => t.Invoke
        // );
//
        // var ns = QtNamespace.FromGeneratorAssemblyName("Generated", [cls]);
        // var file = QtSourceFile.CreateObfuscated("Coolio").WithNamespace(ns);
//
        // ctx.AddFile(file);

        return node.AsIntercept().WithState((LambdaExpressionSyntax) node.Invocation.ArgumentList.Arguments[0].Expression);
    }
}