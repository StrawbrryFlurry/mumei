using System.Reflection;

namespace Mumei.CodeGen.Qt.Qt;

public abstract class QtInterceptorMethodTemplate : IQtInterceptorMethodTemplate {
    public const string MetadataName = "Mumei.CodeGen.Qt.Qt.QtInterceptorMethodTemplate";

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
public delegate TReturn DeclareQtInterceptorMethodWithRefs<TTemplateReferences, TReturn>(QtDynamicInterceptorMethodCtx ctx, TTemplateReferences refs);
public delegate void DeclareQtInterceptorVoidMethodWithRefs<TTemplateReferences>(QtDynamicInterceptorMethodCtx ctx, TTemplateReferences refs);

public readonly ref struct QtDynamicInterceptorMethodCtx {
    public object[] InvocationArguments { get; }
    public MethodInfo Method { get; }

    public IQtThis This { get; }

    public T Invoke<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    /// <summary>
    /// Allows users to initialize a type at compile time,
    /// using generic <see cref="Arg.T1"/> arguments as placeholders
    /// to instantiate <paramref name="instance"/>, whose actual instantiation
    /// will be replaced with the type arguments provided in <paramref name="types"/>.
    /// </summary>
    /// <example>
    /// <code>
    ///  var instance = ctx.Construct&lt;MyType&lt;BestMatchingTypeArg&gt;&gt;([ refs.Type1 ], new MyType&lt;Arg.T1&gt;());
    /// </code>
    /// </example>
    /// <param name="types"></param>
    /// <param name="instance"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="CompileTimeComponentUsedAtRuntimeException"></exception>
    /// TODO: We should prolly just find a way to neatly integrate Generics with the <see cref="QtInterceptorMethodTemplate"/>
    public T Construct<T>(IQtType[] types, object instance) {
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