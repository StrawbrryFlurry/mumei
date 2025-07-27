using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.RoslynAsExpressionReplacement;
using Mumei.CodeGen.Qt.Tests.Setup;
using Mumei.CodeGen.Qt.Tests.SyntaxTreeAsExpressionReplacement;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests;

public sealed class CompilationScopeTests {
    [Fact]
    public void CompilationScope() {
        var result = new SourceGeneratorTest<CompilationScopeTestSourceGenerator>(SourceCode.Of<CompilationTestSource>()).Run();
        result.PassesAssemblyAction(x => {
            var ts = Activator.CreateInstance(x.GetType(nameof(CompilationTestSource)))!;
            var result = ts.GetType().GetMethod(nameof(CompilationTestSource.TestInvocation)).Invoke(ts, []);
            var r = Unsafe.As<CompilationTestSource.TestReceivable>(result);

            Assert.Equal("s", r.ParameterName);
            Assert.Equal("s => s.Length > 0", r.Body);
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
        var results = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, ct) => Filter(node, ct),
            static (ctx, cancellationToken) => Bind(ctx.Node, ctx.SemanticModel, cancellationToken)
        ).Where(static x => x is not null);

        context.RegisterSourceOutput(results, static (context, result) => Execute(context, result!));
    }

    private static bool Filter(SyntaxNode node, CancellationToken cancellationToken) {
        if (node is not InvocationExpressionSyntax invocation) {
            return false;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax) {
            return false;
        }

        if (invocation.ArgumentList.Arguments.Count != 1) {
            return false;
        }

        if (invocation.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax) {
            return false;
        }

        return true;
    }

    private static BoundCompilationResult? Bind(SyntaxNode node, SemanticModel sm, CancellationToken cancellationToken) {
        if (node is not InvocationExpressionSyntax invocation) {
            return null;
        }

        if (invocation.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax lambda) {
            return null;
        }

        if (ModelExtensions.GetSymbolInfo(sm, invocation).Symbol is not IMethodSymbol invokedMethod) {
            return null;
        }

        var containingType = invokedMethod.ContainingType;
        var implementsExpressionReceivable = containingType.AllInterfaces.Any(x => x.MetadataName == "IRoslynExpressionReceivable`1");
        if (!implementsExpressionReceivable) {
            return null;
        }

        var cls = new QtClass(AccessModifier.FileStatic, "");

        cls.BindDynamicTemplateInterceptMethod(
            invocation,
            new { lambda },
            (ctx, refs) => {
                var px = (LambdaExpressionSyntax)SyntaxFactory.ParseExpression(refs.lambda.ToFullString());
                StatementSyntax[] statements = px.Body is BlockSyntax block
                    ? [..block.Statements]
                    : [SyntaxFactory.ExpressionStatement((ExpressionSyntax)px.Body)];
                var x = new RoslynExpression<Action> {
                    Parameters = [],
                    Statements = statements
                };

                ctx.This.Is<IRoslynExpressionReceivable<Action>>().ReceiveExpression(x);
            });

        var boundResult = new BoundCompilationResult();
        return boundResult;
    }

    private static void Execute(SourceProductionContext context, BoundCompilationResult result) {
        if (result.Diagnostics is { Length: > 0 } diagnostics) {
            foreach (var diagnostic in diagnostics) {
                context.ReportDiagnostic(diagnostic);
            }
        }

        if (result.GeneratedFiles is { Length: > 0 } generatedFiles) {
            foreach (var (filePath, text) in generatedFiles) {
                context.AddSource(filePath, text);
            }
        }
    }

    private class BoundCompilationResult {
        private ImmutableArray<Diagnostic>.Builder? _diagnostics;
        public ImmutableArray<Diagnostic>? Diagnostics => _diagnostics?.ToImmutable();

        private ImmutableArray<(string FilePath, SourceText text)>.Builder? _generatedFiles;
        public ImmutableArray<(string FilePath, SourceText text)>? GeneratedFiles => _generatedFiles?.ToImmutable();
    }
}