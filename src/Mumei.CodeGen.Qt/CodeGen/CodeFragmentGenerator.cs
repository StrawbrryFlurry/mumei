using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;
using Mumei.Roslyn.Common;

namespace Mumei.CodeGen.Qt;

[Generator]
internal sealed class CodeFragmentGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var codeFragmentInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            IsCodeFragmentInvocation,
            TranslateCodeFragmentInvocationProvider
        ).Where(x => x);

        var interceptorMethods = codeFragmentInvocations.Select(CreateCodeFragmentInterceptorMethod);

        context.RegisterSourceOutput(interceptorMethods, (ctx, state) => {
            var method = state;

            var cls = SynthesizedClassDeclaration.Create($"CodeFragments_{method.Name}", accessModifier: AccessModifier.FileSealed, methods: [method], renderFeatures: [CodeGenFeature.Interceptors]);
            var ns = SynthesizedNamespace.Create("Generated", [cls]);
            var fileTree = new SourceFileRenderTreeBuilder();

            var content = fileTree.RenderRootNode(ns);

            ctx.AddSource("CodeFragments.g.cs", content);
        });
    }

    private static bool IsCodeFragmentInvocation(SyntaxNode node, CancellationToken _) {
        return SyntaxNodeFilter.IsInvocationOf(node, nameof(CodeFragment.Create), nameof(CodeFragment));
    }

    private static InterceptInvocationIntermediateNode<LambdaExpressionSyntax> TranslateCodeFragmentInvocationProvider(GeneratorSyntaxContext context, CancellationToken _) {
        if (!SyntaxProviderFactory.TryCreateInvocation(context, out var invocation)) {
            return IntermediateNode.None;
        }

        if (!invocation.MethodInfo.IsDeclaredIn<CodeFragment>()) {
            return IntermediateNode.None;
        }

        if (invocation.Operation.Arguments is not [{ Value: IDelegateCreationOperation { Syntax: LambdaExpressionSyntax fragmentDeclaration } }]) {
            return IntermediateNode.None;
        }

        return invocation.AsIntercept().WithState(fragmentDeclaration);
    }

    private static SynthesizedMethodDeclaration CreateCodeFragmentInterceptorMethod(InterceptInvocationIntermediateNode<LambdaExpressionSyntax> codeFragment, CancellationToken _) {
        var name = $"Intercept_CodeFragment_{Math.Abs(codeFragment.Location.GetHashCode())}";
        ImmutableArray<SynthesizedParameter> parameters = [
            new() {
                Name = "declareFragment",
                Type = typeof(Action),
                Attributes = ParameterAttributes.None
            }
        ];

        var declaredBody = new GloballyQualifyingSyntaxRewriter(codeFragment.SemanticModel).Visit(codeFragment.State.Body);
        var bodyContent = declaredBody.NormalizeWhitespace() is BlockSyntax blockSyntax
            ? blockSyntax.Statements.ToFullString()
            : declaredBody.ToFullString();

        var fragment = LiteralNode.ForRawString(bodyContent.TrimEnd('\r', '\n'));
        return SynthesizedMethodDeclaration.Create(
            [],
            AccessModifier.InternalStatic,
            typeof(CodeFragment),
            name,
            [],
            parameters,
            fragment,
            static (f, tree) => {
                tree.Text("return ");
                tree.QualifiedTypeName<CodeFragment>();
                tree.Interpolate($".{nameof(CodeFragment.λCreate)}({f});");
            }
        );
    }
}

/// <summary>
/// Represents a fragment of code that can be used in generated code.
/// <code>
/// CodeFragment.Create(() => {
///     // Code to be included in the generated output
/// });
/// </code>
/// </summary>
public sealed class CodeFragment {
    private readonly string _code;

    public static CodeFragment Create(Action declareFragment) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static CodeFragment Create<TDeps>(TDeps deps, Action<TDeps> declareFragment) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static CodeFragment Create(Func<Task> declareAsyncFragment) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static CodeFragment Create<TDeps>(TDeps deps, Func<TDeps, Task> declareAsyncFragment) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public static CodeFragment λCreate(string code) {
        return new CodeFragment(code);
    }

    private CodeFragment(string code) {
        _code = code;
    }

    public override string ToString() {
        return _code;
    }
}