using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.Roslyn.Components;

public static class SyntheticClassBuilderExtensions {
    extension<TClass>(ISyntheticClassBuilder<TClass> classBuilder) {
        public ISyntheticClassBuilder<TClass> WithTypeParametersFrom(INamedTypeSymbol typeSymbol) {
            var ctx = classBuilder.ΦCompilerApi.Context;
            var typeParameters = ctx.TypeParameterListFrom(typeSymbol);
            classBuilder.WithTypeParameters(typeParameters);
            return classBuilder;
        }
    }

    extension(ISimpleSyntheticClassBuilder classBuilder) {
        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareUnconstructedInterceptorMethod(
            SyntheticIdentifier name,
            InvocationExpressionSyntax invocationToIntercept
        ) {
            var (ctx, builder, methodSymbol) = MakeInterceptorMethodBuilder(name, invocationToIntercept, classBuilder);

            ApplyMethodSignatureToBuilder(ctx, builder, methodSymbol);
            RewriteInterceptorMethod(builder, methodSymbol, invocationToIntercept);

            classBuilder.DeclareMethod(builder);

            return builder;
        }

        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod(
            SyntheticIdentifier name,
            InvocationExpressionSyntax invocationToIntercept
        ) {
            var (ctx, builder, methodSymbol) = MakeInterceptorMethodBuilder(name, invocationToIntercept, classBuilder);

            ApplyConstructedMethodSignatureToBuilder(ctx, builder, methodSymbol);
            RewriteInterceptorMethod(builder, methodSymbol, invocationToIntercept);

            classBuilder.DeclareMethod(builder);

            return builder;
        }

        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod<TMethodDefinition>(
            SyntheticIdentifier name,
            InvocationExpressionSyntax invocationToIntercept,
            Func<TMethodDefinition, Delegate> methodSelector,
            Action<TMethodDefinition>? inputBinder = null
        ) where TMethodDefinition : SyntheticInterceptorMethodDefinition, new() {
            throw new NotImplementedException();
        }
    }

    private static (ICodeGenerationContext ctx, SyntheticInterceptorMethodBuilder<Delegate> builder, IMethodSymbol methodSymbol) MakeInterceptorMethodBuilder(
        SyntheticIdentifier name,
        InvocationExpressionSyntax invocationToIntercept,
        ISimpleSyntheticClassBuilder classBuilder
    ) {
        var context = classBuilder.ΦCompilerApi.Context;
        var builder = new SyntheticInterceptorMethodBuilder<Delegate>(name, classBuilder, classBuilder.ΦCompilerApi.Context);
        var compilation = context.GetContextProvider<CompilationCodeGenerationContextProvider>().Compilation;

        var methodSymbol = compilation.GetSemanticModel(invocationToIntercept.SyntaxTree).GetSymbolInfo(invocationToIntercept).Symbol as IMethodSymbol;
        if (methodSymbol is null) {
            throw new InvalidOperationException("Could not resolve method symbol for interception target invocation.");
        }

        return (context, builder, methodSymbol);
    }

    private static void ApplyMethodSignatureToBuilder<TSignature>(
        ICodeGenerationContext ctx,
        ISyntheticInterceptorMethodBuilder<TSignature> builder,
        IMethodSymbol method
    ) where TSignature : Delegate {
        var parameterList = ctx.ParameterListFrom(method);
        builder.WithParameters(parameterList);

        var typeParameters = ctx.TypeParameterListFrom(method);
        builder.WithTypeParameters(typeParameters);

        var returnType = ctx.Type(method.ReturnType);
        builder.WithReturnType(returnType);
    }

    private static void ApplyConstructedMethodSignatureToBuilder<TSignature>(
        ICodeGenerationContext ctx,
        ISyntheticInterceptorMethodBuilder<TSignature> builder,
        IMethodSymbol method
    ) where TSignature : Delegate {
        if (method.TypeParameters.IsEmpty) {
            ApplyMethodSignatureToBuilder(ctx, builder, method);
            return;
        }

        if (ContainsNonConstructableTypeParameter(method)) {
            ApplyArtificiallyConstructedMethodSignatureToBuilder(ctx, builder, method);
            return;
        }

        Debug.Assert(method.IsGenericMethod && method.TypeArguments.Length == method.TypeParameters.Length, "Method is not a constructed generic method.");
        var parameterList = ctx.ParameterListFrom(method);
        builder.WithParameters(parameterList);

        var returnType = ctx.Type(method.ReturnType);
        builder.WithReturnType(returnType);
    }

    /// <summary>
    /// Ensures that the method signature is valid even when some type parameters cannot be resolved to
    /// their bound types e.g. when they are anonymous types or inaccessible at the declaration site.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="builder"></param>
    /// <param name="method"></param>
    /// <typeparam name="TSignature"></typeparam>
    private static void ApplyArtificiallyConstructedMethodSignatureToBuilder<TSignature>(
        ICodeGenerationContext ctx,
        ISyntheticInterceptorMethodBuilder<TSignature> builder,
        IMethodSymbol method
    ) where TSignature : Delegate {
        using var parameterBuilder = new ArrayBuilder<ISyntheticParameter>();

        for (var i = 0; i < method.Parameters.Length; i++) {
            var parameter = method.Parameters[i];
            var originalParameterType = method.ConstructedFrom.Parameters[i].Type;
            if (parameter.Type.IsAnonymousType) {
                var type = EmitAnonymousTypeAccessProxyForParameter(ctx, builder, parameter);
                parameterBuilder.Add(
                    ctx.Parameter(originalParameterType, $"{Strings.PrivateLocal}AnonymousType__{parameter.Name}", RoslynSyntheticParameter.GetParameterAttributes(parameter))
                );
                continue;
            }

            var syntheticParameter = ctx.Parameter(originalParameterType, parameter.Name, RoslynSyntheticParameter.GetParameterAttributes(parameter));
            parameterBuilder.Add(syntheticParameter);
        }

        builder.WithParameters(parameterBuilder.Elements);

        builder.WithTypeParameters(ctx.TypeParameterListFrom(method));

        builder.WithReturnType(method.ReturnType);
    }

    private static ISyntheticType EmitAnonymousTypeAccessProxyForParameter<TSignature>(
        ICodeGenerationContext ctx,
        ISyntheticInterceptorMethodBuilder<TSignature> builder,
        IParameterSymbol parameter
    ) where TSignature : Delegate {
        if (builder.ContainingType is not ISimpleSyntheticClassBuilder classBuilder) {
            throw new NotSupportedException("Interceptor methods can only be declared in classes.");
        }

        var proxy = classBuilder.DeclareNestedClass<CompileTimeUnknown>(
            classBuilder.MakeUniqueName(
                $"{Strings.PrivateIdentifier}AnonymousTypeAccessProxy_{parameter.Name}"
            )
        ).WithAccessibility(AccessModifier.Private + AccessModifier.Sealed);

        foreach (var member in parameter.Type.GetMembers()) {
            if (member is not IPropertySymbol propertySymbol) {
                continue;
            }

            proxy.DeclareProperty<CompileTimeUnknown>(
                ctx.Type(propertySymbol.Type),
                propertySymbol.Name,
                new SyntheticPropertyAccessorList {
                    Getter = SyntheticPropertyAccessor.SimpleGetter
                }
            ).WithAccessibility(AccessModifier.Public);
        }

        return proxy;
    }

    private static bool ContainsNonConstructableTypeParameter(
        IMethodSymbol method
    ) {
        foreach (var methodTypeParameter in method.TypeArguments) {
            if (methodTypeParameter.IsAnonymousType) {
                return true;
            }

            // TODO: If the type is private and not accessible from this context, we also need to treat it as non-constructable.
        }

        return false;
    }

    private static void RewriteInterceptorMethod<TSignature>(
        ISyntheticInterceptorMethodBuilder<TSignature> builder,
        IMethodSymbol method,
        InvocationExpressionSyntax invocationToIntercept
    ) where TSignature : Delegate {
        var ctx = builder.ΦCompilerApi.Context;
        var interceptAttribute = ctx.InterceptLocationAttribute(invocationToIntercept);
        builder.WithAttributes(interceptAttribute);

        if (!method.IsStatic) {
            var receiverParameter = ctx.Parameter(method.ReceiverType, $"{Strings.PrivateLocal}this", ParameterAttributes.This);
            builder.Parameters.InsertAt(0, receiverParameter);
        }

        builder.WithAccessibility(AccessModifier.Internal + AccessModifier.Static);
    }
}