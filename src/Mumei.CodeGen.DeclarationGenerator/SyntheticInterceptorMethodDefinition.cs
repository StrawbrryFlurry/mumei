using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.DeclarationGenerator;

public abstract class SyntheticInterceptorMethodDefinition : SyntheticDeclarationDefinition {
    public object[] InvocationArguments { get; } = null!;
    public MethodInfo Method { get; } = null!;

    public virtual void BindDynamicComponents() { }

    public TResult Invoke<TResult>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void Invoke() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public virtual ISyntheticInterceptorMethodBuilder<Delegate> InternalBindCompilerMethod(
        ISimpleSyntheticClassBuilder φbuilder,
        IInvocationOperation invocationToBind,
        InterceptableLocation locationToIntercept,
        Delegate φtargetMethod
    ) {
        throw new InvalidOperationException("Method body generation not implemented.");
    }
}

public abstract class SyntheticAsyncInterceptorMethodDefinition : SyntheticDeclarationDefinition {
    public virtual void BindDynamicComponents() { }

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