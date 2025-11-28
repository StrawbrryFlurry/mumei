using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Components;

public abstract class SyntheticInterceptorMethodDefinition {
    public virtual void BindDynamicComponents(BindingContext ctx) { }

    public abstract ISyntheticCodeBlock GenerateMethodBody();

    public TResult Invoke<TResult>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void Invoke() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; } = null!;
    public MethodInfo Method { get; } = null!;

    public readonly struct BindingContext {
        public void Bind<T>(ISyntheticType type) { }
        public void Bind<T>(ITypeSymbol type) { }
        public void Bind<T>(Type type) { }
    }
}

public abstract class SyntheticAsyncInterceptorMethodDefinition {
    public virtual void BindDynamicComponents(BindingContext ctx) { }

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

    public readonly struct BindingContext {
        public void Bind<T>(ISyntheticType type) { }
        public void Bind<T>(ITypeSymbol type) { }
        public void Bind<T>(Type type) { }
    }
}