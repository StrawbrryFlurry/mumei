using System.Reflection;

namespace Mumei.CodeGen.Qt.Qt;

public abstract class QtInterceptorMethodTemplate : IQtInterceptorMethodTemplate {
    public T Invoke<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; set; }
    public MethodInfo Method { get; set; }

    public T Is<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public abstract class QtAsyncInterceptorMethodTemplate : IQtInterceptorMethodTemplate {
    public Task<T> InvokeAsync<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; set; }
    public MethodInfo Method { get; set; }

    public CancellationToken CancellationToken { get; set; }

    public T Is<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public interface IQtInterceptorMethodTemplate : IQtThis;

public delegate TReturn DeclareQtInterceptorMethod<TReturn>(QtDynamicInterceptorMethodCtx ctx);

public delegate void DeclareQtInterceptorVoidMethod(QtDynamicInterceptorMethodCtx ctx);

public delegate TReturn DeclareQtInterceptorMethodWithRefs<TReturn, TTemplateReferences>(QtDynamicInterceptorMethodCtx ctx, TTemplateReferences refs);

public delegate void DeclareQtInterceptorVoidMethodWithRefs<TTemplateReferences>(QtDynamicInterceptorMethodCtx ctx, TTemplateReferences refs);

public readonly ref struct QtDynamicInterceptorMethodCtx {
    public object[] InvocationArguments { get; }
    public MethodInfo Method { get; }

    public IQtThis This { get; }

    public T Invoke<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public CompileTimeUnknown Return<T>(T value) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }


    public CompileTimeUnknown Return() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public readonly ref struct QtDynamicAsyncInterceptorMethodCtx {
    public object[] InvocationArguments { get; }
    public MethodInfo Method { get; }

    public CancellationToken CancellationToken { get; }

    public Task<T> InvokeAsync<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public CompileTimeUnknown Return<T>(T valueOrVoid) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}