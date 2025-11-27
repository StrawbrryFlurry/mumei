using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen.Components.Methods;

public interface ISyntheticMethodBuilder<TSignature> : ISyntheticMethod<TSignature> where TSignature : Delegate {
    [Experimental(Diagnostics.InternalFeatureId)]
    public λIInternalMethodBuilderCompilerApi λCompilerApi { get; }

    public ISyntheticMethodBuilder<TSignature> WithName(string name);

    public ISyntheticMethodBuilder<TSignature> WithBody(TSignature bodyImpl);
    public ISyntheticMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Func<TInputs, TSignature> bodyImpl);
    public ISyntheticMethodBuilder<TSignature> WithBody(ISyntheticCodeBlock body);

    public ISyntheticMethodBuilder<TSignature> WithAccessibility(AccessModifierList modifiers);
    public ISyntheticMethodBuilder<TSignature> WithParameters(params ReadOnlySpan<ISyntheticParameter> parameterList);
    public ISyntheticMethodBuilder<TSignature> WithParameters(ISyntheticParameterList parameterList);
    public ISyntheticMethodBuilder<TSignature> WithTypeParameters(ISyntheticTypeParameterList typeParameterList);
    public ISyntheticMethodBuilder<TSignature> WithTypeParameters(params ReadOnlySpan<ISyntheticTypeParameter> typeParameterList);

    public ISyntheticMethodBuilder<TSignature> WithAttributes(ISyntheticAttributeList attributes);
    public ISyntheticMethodBuilder<TSignature> WithAttributes(params ReadOnlySpan<ISyntheticAttribute> attributes);

    public ISyntheticMethodBuilder<TSignature> WithReturnType(ISyntheticType returnType);
}

/// <summary>
/// API Surface required by the compiler implementation to declare synthetic components.
/// </summary>
// ReSharper disable once InconsistentNaming
[Experimental(Diagnostics.InternalFeatureId)]
public interface λIInternalMethodBuilderCompilerApi {
    public void ApplyMethodSignatureToBuilder<TSignature>(
        ISyntheticMethodBuilder<TSignature> builder,
        MethodInfo methodInfo
    ) where TSignature : Delegate;

    public void ApplyMethodSignatureToBuilder<TSignature>(
        ISyntheticMethodBuilder<TSignature> builder,
        IMethodSymbol method
    ) where TSignature : Delegate;

    public void ApplyMethodSignatureToBuilder<TSignature>(
        ISyntheticInterceptorMethodBuilder<TSignature> builder,
        MethodInfo methodInfo
    ) where TSignature : Delegate;

    public void ApplyMethodSignatureToBuilder<TSignature>(
        ISyntheticInterceptorMethodBuilder<TSignature> builder,
        IMethodSymbol method
    ) where TSignature : Delegate;

    public void ApplyConstructedMethodSignatureToBuilder<TSignature>(
        ISyntheticInterceptorMethodBuilder<TSignature> builder,
        IMethodSymbol method
    ) where TSignature : Delegate;


    public ISyntheticCodeBlock CreateRendererCodeBlock(RenderFragment renderCodeBlock);
    public ISyntheticCodeBlock CreateRendererCodeBlock<TState>(RenderFragment<TState> renderCodeBlock);
}

public interface ISyntheticInterceptorMethodBuilder<TSignature> : ISyntheticMethod<TSignature> where TSignature : Delegate {
    [Experimental(Diagnostics.InternalFeatureId)]
    public λIInternalMethodBuilderCompilerApi λCompilerApi { get; }

    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(TSignature bodyImpl);
    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(Func<IInterceptedMethodContext, TSignature> bodyImpl);
    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(Action<IInterceptedMethodContext> bodyImpl);
    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Func<TInputs, IInterceptedMethodContext, TSignature> bodyImpl);
    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Action<TInputs, IInterceptedMethodContext> bodyImpl);
    public ISyntheticInterceptorMethodBuilder<TSignature> WithBody(ISyntheticCodeBlock body);

    public ISyntheticInterceptorMethodBuilder<TSignature> WithAccessibility(AccessModifierList modifiers);

    public ISyntheticInterceptorMethodBuilder<TSignature> WithParameters(params ReadOnlySpan<ISyntheticParameter> parameterList);
    public ISyntheticInterceptorMethodBuilder<TSignature> WithParameters(ISyntheticParameterList parameterList);
    public ISyntheticInterceptorMethodBuilder<TSignature> WithTypeParameters(ISyntheticTypeParameterList typeParameterList);
    public ISyntheticInterceptorMethodBuilder<TSignature> WithTypeParameters(params ReadOnlySpan<ISyntheticTypeParameter> typeParameterList);

    public ISyntheticInterceptorMethodBuilder<TSignature> WithAttributes(ISyntheticAttributeList attributes);
    public ISyntheticInterceptorMethodBuilder<TSignature> WithAttributes(params ReadOnlySpan<ISyntheticAttribute> attributes);

    public ISyntheticInterceptorMethodBuilder<TSignature> WithReturnType(ISyntheticType returnType);
}

public interface IInterceptedMethodContext {
    public object[] InvocationArguments { get; }
    public MethodInfo Method { get; }

    public TThis This<TThis>();

    public T Invoke<T>();
    public CompileTimeUnknown Invoke();
}