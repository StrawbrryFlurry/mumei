using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Components;

public abstract class SyntheticInterceptorMethodDefinition : ISyntheticMethodDefinition {
    public object[] InvocationArguments { get; } = null!;
    public MethodInfo Method { get; } = null!;

    public virtual void BindDynamicComponents(MethodDefinitionBindingContext ctx) { }

    public virtual ISyntheticMethodBuilder<Delegate> InternalBindCompilerMethod(
        ISimpleSyntheticClassBuilder builder,
        MethodDefinitionBindingContext bindingContext,
        Delegate targetMethod
    ) {
        throw new InvalidOperationException("Method body generation not implemented.");
    }

    public TResult Invoke<TResult>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void Invoke() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public abstract class SyntheticAsyncInterceptorMethodDefinition {
    public virtual void BindDynamicComponents(MethodDefinitionBindingContext ctx) { }

    public abstract ISyntheticCodeBlock GenerateMethodBody();

    public Task<TResult> Invoke<TResult>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public Task Invoke() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; } = null!;
    public MethodInfo Method { get; } = null!;
    public CancellationTokenSource CancellationTokenSource { get; } = new();

    public bool InvocationCanBeCancelled { get; }
}