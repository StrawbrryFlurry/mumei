using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn;
using Mumei.CodeGen.Roslyn.Components;
using Mumei.CodeGen.Roslyn.RoslynCodeProviders;

namespace Mumei.CodeGen.DeclarationGenerator;

// Responsible for generating the interceptors for synthetic class methods e.g. WithBody()
[Generator]
internal sealed class SyntheticClassMethodDeclarationGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var withBodyInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            IsWithBodyInvocation,
            TranslateWithBodyInvocationProvider
        ).Where(x => x);

        var simpleWithBodyInvocations = withBodyInvocations.Where(x => x.State.DependencyExpression is null);
        var withBodyInvocationsWithInput = withBodyInvocations.Where(x => x.State.DependencyExpression is not null);

        var simpleInterceptorMethods = simpleWithBodyInvocations.Select(TranslateSimpleWithBodyInvocationProvider);
        var interceptorMethodsWithInputs = withBodyInvocationsWithInput.Select(TranslateWithBodyWithInputsInvocationProvider);
    }

    private static bool IsWithBodyInvocation(SyntaxNode node, CancellationToken _) {
        return SyntaxNodeFilter.IsInvocationOf(node, "WithBody");
    }

    private static InterceptInvocationIntermediateNode<(ExpressionSyntax? DependencyExpression, LambdaExpressionSyntax BodyOrBodyWithInputsDeclaration)> TranslateWithBodyInvocationProvider(
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

        // The dependency could be any expression and we need to figure out ad-hoc how to resolve it later.
        if (arguments is [{ Value.Syntax: ExpressionSyntax inputExpression }, { Value: IDelegateCreationOperation { Syntax: LambdaExpressionSyntax bodyWithInputsDeclaration } }]) {
            return invocation.AsIntercept().WithState((inputExpression, bodyWithInputsDeclaration))!;
        }

        return IntermediateNode.None;
    }

    private const string InputArgument = "λ__input";
    private const string RenderFragmentInputsArg = "λ__renderTreeInputArg";
    private const string RenderTreeArg = "λ__renderTreeArg";

    private static (MethodDeclarationFragment, ClassDeclarationFragment? InputAccessorClass) TranslateWithBodyWithInputsInvocationProvider(
        InterceptInvocationIntermediateNode<(ExpressionSyntax? InputExpression, LambdaExpressionSyntax BodyOrBodyWithInputsDeclaration)> invocation,
        CancellationToken _
    ) {
        var (inputExpression, inputBodyDeclarationExpression) = invocation.State;
        if (!inputBodyDeclarationExpression.Modifiers.Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword))) {
            // return default!; // Diagnostic: Body lambda must be static
        }

        // We don't want outer block bodies since they could introduce additional locals that we can't resolve.
        if (inputBodyDeclarationExpression.Body is not LambdaExpressionSyntax bodyDeclaration) {
            throw new NotSupportedException("Only simple lambda bodies are supported in WithBody() invocations.");
            // return;
        }

        var inputDeclarationFunc = invocation.SemanticModel.GetOperation(inputBodyDeclarationExpression) as IAnonymousFunctionOperation;

        var methodName = $"Synthetic_WithBody_Interceptor_{Guid.NewGuid():N}";

        var normalizedInputExpression = "λ__input";
        var normalizedInputType = "Tλ__InputArg";
        var inputType = invocation.SemanticModel.GetTypeInfo(inputExpression!);
        ClassDeclarationFragment? inputAccessorClass = null;
        if (inputType.Type.IsAnonymousType) {
            var accessorClassName = $"AnonymousOrInternalInputArgumentAccessor_{methodName}";

            var properties = inputType.Type.GetMembers().Where(x => x is IPropertySymbol).Cast<IPropertySymbol>().ToArray();
            var propertyMembers = ImmutableArray.CreateBuilder<PropertyDeclarationFragment>(properties.Length);
            foreach (var inputMemberProperty in properties) {
                propertyMembers.Add(PropertyDeclarationFragment.Create(
                    AccessModifier.Public,
                    inputMemberProperty.Type.ToRenderFragment(),
                    inputMemberProperty.Name,
                    PropertyDeclarationFragment.AccessorFragment.Get()
                ));
            }

            inputAccessorClass = ClassDeclarationFragment.Create(accessorClassName, accessModifier: AccessModifier.Private + AccessModifier.Sealed, properties: propertyMembers.MoveToImmutable());
            normalizedInputType = accessorClassName;
            normalizedInputExpression = $"System.Runtime.CompilerServices.{nameof(Unsafe)}.{nameof(Unsafe.As)}<Tλ__Input, {accessorClassName}>(ref λ__input)";
        }

        var bodyDeclarationOperation = invocation.SemanticModel.GetOperation(bodyDeclaration) as IAnonymousFunctionOperation;

        var codeBlock = SyntheticRenderCodeBlockSyntaxRewriter.CreateSyntheticRenderBlock(
            bodyDeclaration.Body,
            bodyDeclarationOperation.Symbol.ReturnType,
            invocation.SemanticModel,
            new SyntheticCodeBlockFromLambdaWithInputsResolutionContext(
                invocation.SemanticModel,
                inputDeclarationFunc.Symbol.Parameters[0].Name,
                RenderFragmentInputsArg,
                RenderTreeArg
            )
        );
        var methodDecl = MethodDeclarationFragment.Create(
            [AttributeFragment.Intercept(invocation.Location)],
            AccessModifier.Internal + AccessModifier.Static,
            [TypeParameterFragment.Create("Tλ__MethodSignature", out var tMethodSignature, typeof(Delegate)), TypeParameterFragment.Create("Tλ__Input", out var tInput)],
            TypeInfoFragment.ConstructGenericType(typeof(ISyntheticMethodBuilder<>), tMethodSignature),
            methodName,
            [
                ParameterFragment.Create(TypeInfoFragment.ConstructGenericType(typeof(ISyntheticMethodBuilder<>), tMethodSignature), "λ__this", ParameterAttributes.This, out var thisParm),
                ParameterFragment.Create(tInput, "λ__input"),
                ParameterFragment.Create(TypeInfoFragment.ConstructGenericType(typeof(Func<>), tInput, tMethodSignature), "λ__codeBlockDeclaration")
            ],
            CodeBlockFragment.Create(codeBlock, (builder, fragment) => {
                builder.Interpolate($"{LocalFragment.Var("λ__normalizedInputs", out var normalizedInput)} = {normalizedInputExpression};");
                builder.NewLine();
                builder.Line($"#pragma warning disable {Diagnostics.InternalFeatureId}");
                builder.Interpolate($"{LocalFragment.Var("λ__bodyDeclaration", out var bodyDeclaration)} = ");
                builder.Interpolate(
                    $"{thisParm}.{nameof(ISyntheticMethodBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticMethodBuilder<>.ΦCompilerApi.Context)}<{normalizedInputType}>(new {typeof(RenderFragment<>)}<{normalizedInputType}>(\n{normalizedInput},\nstatic ({RenderTreeArg}, {RenderFragmentInputsArg}) => {{");
                builder.NewLine();
                builder.StartBlock();
                builder.Text(fragment);
                builder.EndBlock();
                builder.Line("}));");
                builder.Line($"#pragma warning enable {Diagnostics.InternalFeatureId}");
                // TODO: Since we likely don't have a parameter list initialized here we should also
                // override the methods parameter list with the correct parameter names based on the input lambda.
                builder.Interpolate($"{thisParm}.{nameof(ISyntheticMethodBuilder<>.WithBody)}({bodyDeclaration});");
                builder.NewLine();
                builder.Interpolate($"return {thisParm};");
            })
        );

        return (methodDecl, inputAccessorClass);
    }

    private static MethodDeclarationFragment TranslateSimpleWithBodyInvocationProvider(
        InterceptInvocationIntermediateNode<(ExpressionSyntax? InputExpression, LambdaExpressionSyntax BodyOrBodyWithInputsDeclaration)> invocation,
        CancellationToken _
    ) {
        var (_, bodyDeclaration) = invocation.State;
        if (!bodyDeclaration.Modifiers.Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword))) {
            // return default!; // Diagnostic: Body lambda must be static
        }

        var lambda = invocation.SemanticModel.GetOperation(bodyDeclaration) as IAnonymousFunctionOperation;

        var methodName = $"Synthetic_WithBody_Interceptor_{Guid.NewGuid():N}";
        var codeBlock = SyntheticRenderCodeBlockSyntaxRewriter.CreateSyntheticRenderBlock(
            bodyDeclaration.Body,
            lambda.Symbol.ReturnType,
            invocation.SemanticModel,
            new SyntheticCodeBlockFromLambdaResolutionContext(invocation.SemanticModel)
        );
        var methodDecl = MethodDeclarationFragment.Create(
            [AttributeFragment.Intercept(invocation.Location)],
            AccessModifier.Private + AccessModifier.Static,
            [],
            lambda.Symbol.ReturnType.ToRenderFragment(),
            methodName,
            [],
            CodeBlockFragment.Create(codeBlock, (builder, fragment) => {
                builder.Text("var λ__bodyDeclaration = ");
                builder.Interpolate($"{nameof(ISyntheticMethodBuilder<>.ΦCompilerApi)}.{nameof(ISyntheticMethodBuilder<>.ΦCompilerApi.Context.Block)}((renderTree) => {{");
                builder.Block(fragment);
                builder.Text("}));");
                builder.Text($"this.{nameof(ISyntheticMethodBuilder<>.WithBody)}(λ__bodyDeclaration);");
                builder.Text("return this;");
            })
        );

        return methodDecl;
    }
}