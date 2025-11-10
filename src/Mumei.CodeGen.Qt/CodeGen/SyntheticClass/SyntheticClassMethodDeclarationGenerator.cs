using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;

namespace Mumei.CodeGen.Qt;

// Responsible for generating the interceptors for synthetic class methods e.g. WithBody()
internal sealed class SyntheticClassMethodDeclarationGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var codeFragmentInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            IsCodeFragmentInvocation,
            TranslateCodeFragmentInvocationProvider
        ).Where(x => x);

        // var interceptorMethods = codeFragmentInvocations.Select(CreateCodeFragmentInterceptorMethod);
//
        context.RegisterSourceOutput(codeFragmentInvocations, (ctx, state) => { });
    }

    private static bool IsCodeFragmentInvocation(SyntaxNode node, CancellationToken _) {
        return SyntaxNodeFilter.IsInvocationOf(node, "CallSomethingWithTypeSymbol");
    }

    private static InterceptInvocationIntermediateNode<LambdaExpressionSyntax> TranslateCodeFragmentInvocationProvider(GeneratorSyntaxContext context, CancellationToken _) {
        if (!SyntaxProviderFactory.TryCreateInvocation(context, out var invocation)) {
            return IntermediateNode.None;
        }

        var arg = invocation.Invocation.ArgumentList.Arguments[0].Expression;
        var syntheticMacroInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                arg,
                SyntaxFactory.IdentifierName("Bind__self")
            )
        );

        var macroInvocation = context.SemanticModel.GetSpeculativeSymbolInfo(arg.SpanStart, syntheticMacroInvocation, SpeculativeBindingOption.BindAsExpression);
        if (macroInvocation.Symbol is not IMethodSymbol macroBindingMethod) {
            return IntermediateNode.None;
        }

        var __ = macroBindingMethod;

        if (!invocation.MethodInfo.IsDeclaredIn<CodeFragment>()) {
            return IntermediateNode.None;
        }

        if (invocation.Operation.Arguments is not [{ Value: IDelegateCreationOperation { Syntax: LambdaExpressionSyntax fragmentDeclaration } }]) {
            return IntermediateNode.None;
        }

        return invocation.AsIntercept().WithState(fragmentDeclaration);
    }
}