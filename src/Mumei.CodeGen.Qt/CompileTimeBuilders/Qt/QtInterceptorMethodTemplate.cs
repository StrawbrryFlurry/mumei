using System.Reflection;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground.Qt;

public abstract class QtInterceptorMethodTemplate : IQtInterceptorMethodTemplate {
    public T Invoke<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; set; }
    public MethodInfo Method { get; set; }

    public T As<T>() {
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

    public T As<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public abstract class QtDynamicInterceptorMethodTemplate : IQtDynamicInterceptorMethodTemplate {
    protected abstract Arg.TReturn Implementation();

    public T InvokeAsync<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; set; }
    public MethodInfo Method { get; set; }

    public Arg.TReturn Return<T>(T valueOrVoid) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T As<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public abstract class QtDynamicAsyncInterceptorMethodTemplate : IQtDynamicInterceptorMethodTemplate {
    protected abstract Task<Arg.TReturn> Implementation();

    public Task<T> Invoke<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; set; }
    public MethodInfo Method { get; set; }

    public CancellationToken CancellationToken { get; set; }

    public Arg.TReturn Return<T>(T valueOrVoid) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T As<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

public interface IQtInterceptorMethodTemplate : IQtThis;

public interface IQtDynamicInterceptorMethodTemplate : IQtThis;