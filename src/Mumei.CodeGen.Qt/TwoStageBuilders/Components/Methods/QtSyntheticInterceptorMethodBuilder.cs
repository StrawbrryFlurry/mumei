using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticInterceptorMethodBuilder<TSignature>(
    string name,
    IλInternalClassBuilderCompilerApi classApi
) : QtSyntheticMethodBase<ISyntheticInterceptorMethodBuilder<TSignature>>(name, classApi), ISyntheticInterceptorMethodBuilder<TSignature> where TSignature : Delegate {
    public static QtSyntheticInterceptorMethodBuilder<TSignature> CreateFromInterceptionTargetInvocation(
        string name,
        InvocationExpressionSyntax invocationToIntercept,
        IλInternalClassBuilderCompilerApi classApi
    ) {
        var builder = new QtSyntheticInterceptorMethodBuilder<TSignature>(name, classApi);
        var methodSymbol = ModelExtensions.GetSymbolInfo(classApi.Compilation.UnderlyingCompilation.GetSemanticModel(invocationToIntercept.SyntaxTree), invocationToIntercept).Symbol as IMethodSymbol;
        if (methodSymbol is null) {
            throw new InvalidOperationException("Could not resolve method symbol for interception target invocation.");
        }

        // builder.λCompilerApi.ApplyMethodSignatureToBuilder(builder, methodSymbol);
        return builder;
    }

    public static QtSyntheticInterceptorMethodBuilder<TSignature> CreateFromConstructedInterceptionTargetInvocation(
        string name,
        InvocationExpressionSyntax invocationToIntercept,
        IλInternalClassBuilderCompilerApi classApi
    ) {
        var builder = new QtSyntheticInterceptorMethodBuilder<TSignature>(name, classApi);
        var methodSymbol = ModelExtensions.GetSymbolInfo(classApi.Compilation.UnderlyingCompilation.GetSemanticModel(invocationToIntercept.SyntaxTree), invocationToIntercept).Symbol as IMethodSymbol;
        if (methodSymbol is null) {
            throw new InvalidOperationException("Could not resolve method symbol for interception target invocation.");
        }

        // builder.λCompilerApi.ApplyConstructedMethodSignatureToBuilder(builder, methodSymbol);
        return builder;
    }

    public TSignature Bind(object target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TCast BindAs<TCast>(object target) where TCast : Delegate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(TSignature bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(Func<IInterceptedMethodContext, TSignature> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(Action<IInterceptedMethodContext> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Func<TInputs, IInterceptedMethodContext, TSignature> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Action<TInputs, IInterceptedMethodContext> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}