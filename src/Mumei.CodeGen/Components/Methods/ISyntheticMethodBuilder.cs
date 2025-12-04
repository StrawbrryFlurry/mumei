using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Components;

public interface ISyntheticMethodBuilder<TSignature> : ISyntheticMethod<TSignature> where TSignature : Delegate {
    [Experimental(Diagnostics.InternalFeatureId)]
    public IΦInternalMethodBuilderCompilerApi ΦCompilerApi { get; }

    public ISyntheticMethodBuilder<TSignature> WithName(string name);

    public ISyntheticMethodBuilder<TSignature> WithBody(TSignature bodyImpl);
    public ISyntheticMethodBuilder<TSignature> WithBody<TInputs>(TInputs inputs, Func<TInputs, TSignature> bodyImpl);
    public ISyntheticMethodBuilder<TSignature> WithBody(ISyntheticCodeBlock body);

    public ISyntheticMethodBuilder<TSignature> WithAccessibility(AccessModifierList modifiers);
    public ISyntheticMethodBuilder<TSignature> WithParameters(params ReadOnlySpan<ISyntheticParameter> parameterList);
    public ISyntheticMethodBuilder<TSignature> WithParameters(ISyntheticParameterList parameterList);
    [OverloadResolutionPriority(1)]
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
public interface IΦInternalMethodBuilderCompilerApi {
    public ICodeGenerationContext Context { get; }
}

public interface ISyntheticInterceptorMethodBuilder<TSignature> : ISyntheticMethod<TSignature> where TSignature : Delegate {
    [Experimental(Diagnostics.InternalFeatureId)]
    public IΦInternalMethodBuilderCompilerApi ΦCompilerApi { get; }

    public ISyntheticParameterList Parameters { get; }

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