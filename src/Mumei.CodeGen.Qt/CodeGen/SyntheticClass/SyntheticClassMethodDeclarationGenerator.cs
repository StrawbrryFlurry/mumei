using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.TwoStageBuilders.Components;
using Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;
using Mumei.Roslyn.Common;

namespace Mumei.CodeGen.Qt;

// Responsible for generating the interceptors for synthetic class methods e.g. WithBody()
internal sealed class SyntheticClassMethodDeclarationGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var withBodyInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            IsWithBodyInvocation,
            TranslateWithBodyInvocationProvider
        ).Where(x => x);

        var simpleWithBodyInvocations = withBodyInvocations.Where(x => x.State.InputCreationExpression is null);
        var withBodyInvocationsWithState = withBodyInvocations.Where(x => x.State.InputCreationExpression is not null);

        var simpleInterceptorMethods = simpleWithBodyInvocations.Select(TranslateSimpleWithBodyInvocationProvider);
        var simpleInterceptorClass = simpleInterceptorMethods.Collect().Select((x, _) => {
            var classDecl = SynthesizedClassDeclaration.Create(
                "SyntheticClassMethodWithBodyInterceptors",
                accessModifier: AccessModifier.FileStatic,
                methods: x,
                renderFeatures: [CodeGenFeature.Interceptors]
            );
            return classDecl;
        });

        // var interceptorMethods = codeFragmentInvocations.Select(CreateCodeFragmentInterceptorMethod);
//
        context.RegisterSourceOutput(simpleInterceptorClass, (ctx, state) => {
            var filename = "SyntheticClassMethodWithBodyInterceptors.g.cs";
            var renderer = new SourceFileRenderTreeBuilder();
            var ns = SynthesizedNamespace.Create(
                "Generated",
                [state]
            );
            var content = renderer.RenderRootNode(ns);
            ctx.AddSource(filename, content);
        });
    }

    private static bool IsWithBodyInvocation(SyntaxNode node, CancellationToken _) {
        return SyntaxNodeFilter.IsInvocationOf(node, "WithBody");
    }

    private static InterceptInvocationIntermediateNode<(ExpressionSyntax? InputCreationExpression, LambdaExpressionSyntax BodyOrBodyWithInputsDeclaration)>
        TranslateWithBodyInvocationProvider(
            GeneratorSyntaxContext context,
            CancellationToken _
        ) {
        if (!SyntaxProviderFactory.TryCreateInvocation(context, out var invocation)) {
            return IntermediateNode.None;
        }

        if (!invocation.MethodInfo.IsDeclaredInAnyConstructedFormOf<ISyntheticMethodBuilder<Delegate>>()) {
            return IntermediateNode.None;
        }

        var arguments = invocation.Operation.Arguments;

        if (arguments is [{ Value: IDelegateCreationOperation { Syntax: LambdaExpressionSyntax bodyDeclaration } }]) {
            return invocation.AsIntercept().WithState<(ExpressionSyntax?, LambdaExpressionSyntax)>((null, bodyDeclaration));
        }

        if (arguments is [{ Value: IObjectCreationOperation { Syntax: ExpressionSyntax inputCreationExpression } }, { Value: IDelegateCreationOperation { Syntax: LambdaExpressionSyntax bodyWithInputsDeclaration } }]) {
            return invocation.AsIntercept().WithState((inputCreationExpression, bodyWithInputsDeclaration))!;
        }

        return IntermediateNode.None;
    }

    private static SynthesizedMethodDeclaration TranslateSimpleWithBodyInvocationProvider(
        InterceptInvocationIntermediateNode<(ExpressionSyntax? InputCreationExpression, LambdaExpressionSyntax BodyOrBodyWithInputsDeclaration)> invocation,
        CancellationToken _
    ) {
        var (_, bodyDeclaration) = invocation.State;
        if (!bodyDeclaration.Modifiers.Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword))) {
            // return default!; // Diagnostic: Body lambda must be static
        }

        var lambda = invocation.SemanticModel.GetOperation(bodyDeclaration) as IAnonymousFunctionOperation;

        var methodName = $"Synthetic_WithBody_Interceptor_{Guid.NewGuid():N}";
        var methodDecl = SynthesizedMethodDeclaration.Create(
            [SynthesizedAttribute.Intercept(invocation.Location)],
            AccessModifier.PrivateStatic,
            new SynthesizedTypeInfo(lambda.Symbol.ReturnType),
            methodName,
            [],
            [],
            new { },
            (o, renderTree) => {
                var globalizer = new GloballyQualifyingSyntaxRewriter(invocation.SemanticModel);
                renderTree.Text(globalizer.Visit(bodyDeclaration.Body).NormalizeWhitespace().ToFullString());
            }
        );

        return methodDecl;
    }

    // Synthetic symbol binding:
    // var arg = invocation.Invocation.ArgumentList.Arguments[0].Expression;
    // var syntheticMacroInvocation = SyntaxFactory.InvocationExpression(
    //     SyntaxFactory.MemberAccessExpression(
    //         SyntaxKind.SimpleMemberAccessExpression,
    //         arg,
    //         SyntaxFactory.IdentifierName("Bind__self")
    //     )
    // );
    //
    // var macroInvocation = context.SemanticModel.GetSpeculativeSymbolInfo(arg.SpanStart, syntheticMacroInvocation, SpeculativeBindingOption.BindAsExpression);
    //     if (macroInvocation.Symbol is not IMethodSymbol macroBindingMethod) {
    //     return IntermediateNode.None;
    // }
    //
    // var __ = macroBindingMethod;
}