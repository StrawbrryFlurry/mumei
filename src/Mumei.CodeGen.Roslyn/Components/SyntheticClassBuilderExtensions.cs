using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Roslyn.Components;

public static class SyntheticClassBuilderExtensions {
    extension<TClass>(ISyntheticClassBuilder<TClass> classBuilder) {
        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareUnconstructedInterceptorMethod(
            string name,
            InvocationExpressionSyntax invocationToIntercept
        ) {
            return classBuilder.DeclareUnconstructedInterceptorMethod(
                new ConstantSyntheticIdentifier(name),
                invocationToIntercept
            );
        }

        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareUnconstructedInterceptorMethod(
            ISyntheticIdentifier name,
            InvocationExpressionSyntax invocationToIntercept
        ) {
            var (ctx, builder, methodSymbol) = MakeInterceptorMethodBuilder(name, invocationToIntercept, classBuilder.ΦCompilerApi);

            ApplyMethodSignatureToBuilder(ctx, builder, methodSymbol);
            RewriteInterceptorMethod(builder, methodSymbol, invocationToIntercept);

            classBuilder.DeclareMethod(builder);

            return builder;
        }

        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod(
            string name,
            InvocationExpressionSyntax invocationToIntercept
        ) {
            return classBuilder.DeclareInterceptorMethod(
                new ConstantSyntheticIdentifier(name),
                invocationToIntercept
            );
        }

        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod(
            ISyntheticIdentifier name,
            InvocationExpressionSyntax invocationToIntercept
        ) {
            var (ctx, builder, methodSymbol) = MakeInterceptorMethodBuilder(name, invocationToIntercept, classBuilder.ΦCompilerApi);

            ApplyConstructedMethodSignatureToBuilder(ctx, builder, methodSymbol);
            RewriteInterceptorMethod(builder, methodSymbol, invocationToIntercept);

            classBuilder.DeclareMethod(builder);

            return builder;
        }

        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod<TMethodDefinition>(
            string name,
            InvocationExpressionSyntax invocationToIntercept,
            Func<TMethodDefinition, Delegate> methodSelector,
            Action<TMethodDefinition>? inputBinder = null
        ) where TMethodDefinition : SyntheticInterceptorMethodDefinition, new() {
            throw new NotImplementedException();
        }
    }

    private static (ICodeGenerationContext ctx, SyntheticInterceptorMethodBuilder<Delegate> builder, IMethodSymbol methodSymbol) MakeInterceptorMethodBuilder(
        ISyntheticIdentifier name,
        InvocationExpressionSyntax invocationToIntercept,
        IΦInternalClassBuilderCompilerApi classApi
    ) {
        var ctx = classApi.Context;
        var builder = new SyntheticInterceptorMethodBuilder<Delegate>(name, classApi);
        var compilation = ctx.GetContextProvider<CompilationCodeGenerationContextProvider>().Compilation;

        var methodSymbol = compilation.GetSemanticModel(invocationToIntercept.SyntaxTree).GetSymbolInfo(invocationToIntercept).Symbol as IMethodSymbol;
        if (methodSymbol is null) {
            throw new InvalidOperationException("Could not resolve method symbol for interception target invocation.");
        }

        return (ctx, builder, methodSymbol);
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

        Debug.Assert(method.IsGenericMethod && method.TypeArguments.Length == method.TypeParameters.Length, "Method is not a constructed generic method.");
        var parameterList = ctx.ParameterListFrom(method);
        builder.WithParameters(parameterList);

        var returnType = ctx.Type(method.ReturnType);
        builder.WithReturnType(returnType);
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
            var receiverParameter = ctx.Parameter("ϕthis", method.ReceiverType, ParameterAttributes.This);
            builder.Parameters.InsertAt(0, receiverParameter);
        }

        builder.WithAccessibility(AccessModifier.Public + AccessModifier.Static);
    }
}