using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Qt.TwoStageBuilders.Components;
using Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;
using Mumei.Roslyn.Common;

namespace Mumei.CodeGen.Qt;

internal sealed class CodeFragmentGenerator_SyntheticClass : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var codeFragmentInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            IsCodeFragmentInvocation,
            TranslateCodeFragmentInvocationProvider
        ).Where(x => x);

        var interceptorNamespace = codeFragmentInvocations.Collect().Select(CreateCodeFragmentInterceptorMethod);

        context.RegisterSourceOutput(interceptorNamespace, (ctx, state) => {
            if (state.IsEmpty) {
                return;
            }

            var fileTree = new SourceFileRenderTreeBuilder();
            var content = fileTree.RenderRootNode(state);
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

    private static NamespaceFragment CreateCodeFragmentInterceptorMethod(
        ImmutableArray<InterceptInvocationIntermediateNode<LambdaExpressionSyntax>> codeFragments,
        CancellationToken _
    ) {
        if (codeFragments.IsDefaultOrEmpty) {
            return NamespaceFragment.Empty;
        }

        var compilation = new SyntheticCompilation(codeFragments[0].SemanticModel.Compilation);
        var interceptorClass = compilation.DeclareClass(compilation.MakeUniqueName("CodeFragmentInterceptor"))
            .WithModifiers(SyntheticAccessModifier.File, SyntheticAccessModifier.Sealed);

        foreach (var codeFragment in codeFragments) {
            var declaredBody = new GloballyQualifyingSyntaxRewriter(codeFragment.SemanticModel).Visit(codeFragment.State.Body);
            var bodyContent = declaredBody.NormalizeWhitespace() is BlockSyntax blockSyntax
                ? blockSyntax.Statements.ToFullString()
                : declaredBody.ToFullString();

            var fragment = LiteralNode.ForRawString(bodyContent.TrimEnd('\r', '\n'));

            interceptorClass.DeclareInterceptorMethod<SimpleCodeFragmentCreateMethod>(
                    interceptorClass.MakeUniqueName("Intercept_Create"),
                    codeFragment.Invocation,
                    m => {
                        m.Body = new SyntheticRawStringLiteralExpression(bodyContent.TrimEnd('\r', '\n'), Strings.RawStringLiteral12);
                    },
                    m => m.InterceptCreate
                )
                .WithAccessibility(SyntheticAccessModifier.Internal, SyntheticAccessModifier.Static);
        }


        var cls = compilation.Synthesize(interceptorClass);
        return NamespaceFragment.Create("Generated", [cls]);
    }
}

internal sealed class CodeFragmentInterceptor : SyntheticClassDefinition<CodeFragmentInterceptor> {
    [Input]
    public ISyntheticMethod InterceptCreate { get; set; } = null!;

    public override void SetupDynamic(ISyntheticClassBuilder<CodeFragmentInterceptor> builder) {
        // builder.DeclareMethod()
    }
}

internal sealed class SimpleCodeFragmentCreateMethod : SyntheticMethodDefinition {
    [Input]
    public SyntheticRawStringLiteralExpression Body { get; set; } = null!;

    public CodeFragment InterceptCreate(Action declareFragment) {
        return CodeFragment.λCreate(Body);
    }

    //public CodeFragment InterceptCreateCore<TDeps>(TDeps deps, Action<TDeps> declareFragment) { }
    //
    //public CodeFragment InterceptCreateCore(Func<Task> declareAsyncFragment) { }
    //
    //public CodeFragment InterceptCreateCore<TDeps>(TDeps deps, Func<TDeps, Task> declareAsyncFragment) { }
}