using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Roslyn;
using Mumei.CodeGen.Qt.TwoStageBuilders.Components;
using Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;
using Mumei.Roslyn.Common;
using CodeBlockFragment = Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents.CodeBlockFragment;

namespace Mumei.CodeGen.Qt;

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

        var interceptorClassWithInputs = interceptorMethodsWithInputs.Collect().Select((x, _) => {
            var classDecl = ClassDeclarationFragment.Create(
                "SyntheticClassMethodWithBodyInterceptorsWithInputs",
                accessModifier: AccessModifier.FileStatic,
                methods: x.Select(x => x.Item1).ToImmutableArray(),
                nestedClassDeclarations: x.Where(x => x.InputAccessorClass is not null).Select(x => x.InputAccessorClass!.Value).ToImmutableArray(),
                renderFeatures: [CodeGenFeature.Interceptors]
            );
            return classDecl;
        });

        var simpleInterceptorClass = simpleInterceptorMethods.Collect().Select((x, _) => {
            var classDecl = ClassDeclarationFragment.Create(
                "SyntheticClassMethodWithBodyInterceptors",
                accessModifier: AccessModifier.FileStatic,
                methods: x,
                renderFeatures: [CodeGenFeature.Interceptors]
            );
            return classDecl;
        });

        // var interceptorMethods = codeFragmentInvocations.Select(CreateCodeFragmentInterceptorMethod);

        context.RegisterSourceOutput(interceptorClassWithInputs, (ctx, state) => {
            var filename = "SyntheticClassMethodWithBodyInterceptors.g.cs";
            var renderer = new SourceFileRenderTreeBuilder();
            var ns = NamespaceFragment.Create(
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

    private static (MethodDeclarationFragment, ClassDeclarationFragment? InputAccessorClass) TranslateWithBodyWithInputsInvocationProvider(
        InterceptInvocationIntermediateNode<(ExpressionSyntax? InputExpression, LambdaExpressionSyntax BodyOrBodyWithInputsDeclaration)> invocation,
        CancellationToken _
    ) {
        var (inputExpression, bodyDeclaration) = invocation.State;
        if (!bodyDeclaration.Modifiers.Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword))) {
            // return default!; // Diagnostic: Body lambda must be static
        }

        var lambda = invocation.SemanticModel.GetOperation(bodyDeclaration) as IAnonymousFunctionOperation;

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
                    new TypeInfoFragment(inputMemberProperty.Type),
                    inputMemberProperty.Name,
                    PropertyDeclarationFragment.AccessorFragment.Get()
                ));
            }

            inputAccessorClass = ClassDeclarationFragment.Create(accessorClassName, accessModifier: AccessModifier.PrivateSealed, properties: propertyMembers.MoveToImmutable());
            normalizedInputType = accessorClassName;
            normalizedInputExpression = $"System.Runtime.CompilerServices.{nameof(Unsafe)}.{nameof(Unsafe.As)}<Tλ__Input, {accessorClassName}>(ref λ__input)";
        }

        var codeBlock = SyntheticRenderCodeBlockSyntaxRewriter.CreateSyntheticRenderBlock(
            bodyDeclaration.Body,
            invocation.SemanticModel,
            new SyntheticCodeBlockFromLambdaWithInputsResolutionContext(invocation.SemanticModel)
        );
        var methodDecl = MethodDeclarationFragment.Create(
            [AttributeFragment.Intercept(invocation.Location)],
            AccessModifier.PrivateStatic,
            new TypeInfoFragment(invocation.Operation.TargetMethod.ReturnType),
            methodName,
            [TypeParameterFragment.Create("Tλ__Input")],
            [
                ParameterFragment.Create(invocation.Operation.TargetMethod.DeclaringType(), "λ__this", ParameterAttributes.This),
                ParameterFragment.Create(TypeInfoFragment.ForKeyword("Tλ__Input"), "λ__input")
            ],
            CodeBlockFragment.Create((codeBlock as ISyntheticConstructable<CodeBlockFragment>)!.Construct(), (builder, fragment) => {
                builder.Interpolate($"var λ__normalizedInput = {normalizedInputExpression};");
                builder.NewLine();
                builder.Text("var λ__bodyDeclaration = ");
                builder.Interpolate($"λ__this.{nameof(ISyntheticMethodBuilder<>.λCompilerApi)}.{nameof(ISyntheticMethodBuilder<>.λCompilerApi.CreateRendererCodeBlock)}<{normalizedInputType}>(new {typeof(RenderFragment<>)}<{normalizedInputType}>(λ__normalizedInput, (renderTree, λ__renderTreeInputArg) => {{");
                builder.NewLine();
                builder.Node(fragment);
                builder.NewLine();
                builder.Line("}));");
                builder.Line($"λ__this.{nameof(ISyntheticMethodBuilder.WithBody)}(λ__bodyDeclaration);");
                builder.Text("return λ__this;");
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
            invocation.SemanticModel,
            new SyntheticCodeBlockFromLambdaResolutionContext(invocation.SemanticModel)
        );
        var methodDecl = MethodDeclarationFragment.Create(
            [AttributeFragment.Intercept(invocation.Location)],
            AccessModifier.PrivateStatic,
            new TypeInfoFragment(lambda.Symbol.ReturnType),
            methodName,
            [],
            [],
            CodeBlockFragment.Create((codeBlock as ISyntheticConstructable<CodeBlockFragment>)!.Construct(), (builder, fragment) => {
                builder.Text("var λ__bodyDeclaration = ");
                builder.Interpolate($"{nameof(ISyntheticMethodBuilder<>.λCompilerApi)}.{nameof(ISyntheticMethodBuilder<>.λCompilerApi.CreateRendererCodeBlock)}((renderTree) => {{");
                builder.Node(fragment);
                builder.Text("}));");
                builder.Text($"this.{nameof(ISyntheticMethodBuilder.WithBody)}(λ__bodyDeclaration);");
                builder.Text("return this;");
            })
        );

        return methodDecl;
    }
}