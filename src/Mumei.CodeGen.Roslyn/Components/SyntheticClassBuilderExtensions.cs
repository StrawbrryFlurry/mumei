using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.Components;

public static class SyntheticClassBuilderExtensions {
    extension<TClass>(ISyntheticClassBuilder<TClass> classBuilder) {
        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareUnconstructedInterceptorMethod(
            string name,
            InvocationExpressionSyntax invocationToIntercept
        ) {
            var ctx = classBuilder.λCompilerApi.Context;
            var builder = new SyntheticInterceptorMethodBuilder<Delegate>(name, classBuilder.λCompilerApi);
            var compilation = ctx.GetSynthesisProvider<CompilationSynthesisProvider>().Compilation;

            var methodSymbol = compilation.GetSemanticModel(invocationToIntercept.SyntaxTree).GetSymbolInfo(invocationToIntercept).Symbol as IMethodSymbol;
            if (methodSymbol is null) {
                throw new InvalidOperationException("Could not resolve method symbol for interception target invocation.");
            }

            ApplyMethodSignatureToBuilder(ctx, builder, methodSymbol);
            return builder;
        }

        public ISyntheticInterceptorMethodBuilder<Delegate> DeclareInterceptorMethod(
            string name,
            InvocationExpressionSyntax invocationToIntercept
        ) {
            var ctx = classBuilder.λCompilerApi.Context;
            var builder = new SyntheticInterceptorMethodBuilder<Delegate>(name, classBuilder.λCompilerApi);
            var compilation = ctx.GetSynthesisProvider<CompilationSynthesisProvider>().Compilation;

            var methodSymbol = compilation.GetSemanticModel(invocationToIntercept.SyntaxTree).GetSymbolInfo(invocationToIntercept).Symbol as IMethodSymbol;
            if (methodSymbol is null) {
                throw new InvalidOperationException("Could not resolve method symbol for interception target invocation.");
            }

            ApplyConstructedMethodSignatureToBuilder(ctx, builder, methodSymbol);
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
}