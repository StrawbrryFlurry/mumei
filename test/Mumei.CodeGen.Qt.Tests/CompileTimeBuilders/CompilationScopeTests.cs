using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        var cls = new QtClass(AccessModifier.FileStatic, "cls");

        var compilation = sm.Compilation;
        QtCompilationScope.SetActiveScope(compilation);

        cls.BindDynamicTemplateInterceptMethod(
            invocation,
            new { lambda },
            static (ctx, refs) => {
                var px = (LambdaExpressionSyntax)SyntaxFactory.ParseExpression(refs.lambda.NormalizeWhitespace().ToFullString());
                StatementSyntax[] statements = px.Body is BlockSyntax block
                    ? [..block.Statements]
                    : [SyntaxFactory.ExpressionStatement((ExpressionSyntax)px.Body)];

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

        var boundResult = new BoundCompilationResult();

        var ns = new QtNamespace($"{compilation.AssemblyName}.Generated", [cls]);
        var writer = new SyntaxWriter();
        writer.WriteLine(
            """
            #pragma warning disable
            namespace System.Runtime.CompilerServices {
                [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                file sealed class InterceptsLocationAttribute(int version, string data) : Attribute;
            }
            #pragma warning enable
            """
        );
        ns.WriteSyntax(ref writer);
        boundResult.AddResult("out.g.cs", SourceText.From(writer.ToString(), Encoding.UTF8));
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

        public void AddResult(string filePath, SourceText text) {
            _generatedFiles ??= ImmutableArray.CreateBuilder<(string FilePath, SourceText text)>();
            _generatedFiles.Add((filePath, text));
        }
    }
}