using System.ComponentModel;
using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen;

/// <summary>
/// The intercepted call inside an <see cref="InterceptorTemplate"/>. Members like <see cref="Method"/> are evaluated
/// while generating; <see cref="Call{TResult}.Proceed()"/> and <see cref="Call{TResult}.Arg{T}"/> are rewritten to code.
/// </summary>
public abstract class Call {
    private protected Call(InterceptorBuilder interceptor) {
        Interceptor = interceptor;
    }

    private protected InterceptorBuilder Interceptor { get; }

    public CallSite Site => Interceptor.Call;

    /// <summary>The intercepted method.</summary>
    public MethodModel Method => Interceptor.Call.Method;

    public SourceLocation Location => Interceptor.Call.Location;

    /// <summary>The parameters of the interceptor matching the arguments of the call, excluding the receiver.</summary>
    public EquatableArray<ParameterModel> Arguments => Interceptor.Arguments;

    /// <summary>The receiver of the call, if any.</summary>
    public ParameterModel? Receiver => Interceptor.Receiver;
}

/// <inheritdoc />
public sealed class Call<TResult> : Call {
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Call(InterceptorBuilder interceptor) : base(interceptor) { }

    /// <summary>Invokes the intercepted method with the original arguments.</summary>
    public TResult Proceed() {
        throw TemplateOnly();
    }

    /// <summary>Invokes the intercepted method with the given arguments (excluding the receiver).</summary>
    public TResult Proceed(params object?[] arguments) {
        throw TemplateOnly();
    }

    /// <summary>The value of the argument passed for the parameter <paramref name="name"/>.</summary>
    public T Arg<T>(string name) {
        throw TemplateOnly();
    }

    private static InvalidOperationException TemplateOnly() {
        return new InvalidOperationException(
            "Call.Proceed and Call.Arg can only be used in the code emitted by an interceptor template."
        );
    }
}

/// <summary>
/// A type known only while generating, bound to the template type parameter <typeparamref name="T"/>.
/// Inside a template, <typeparamref name="T"/> is replaced with this type in emitted code.
/// </summary>
public readonly record struct Type<T>(TypeRef Ref) : IRenderFragment {
    public string Name => Ref.Name;
    public string Namespace => Ref.Namespace;
    public string FullName => Ref.FullName;
    public string MetadataName => Ref.MetadataName;

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(Ref.FullName);
    }

    public override string ToString() {
        return Ref.FullName;
    }

    public static implicit operator Type<T>(TypeRef type) {
        return new Type<T>(type);
    }

    public static implicit operator Type<T>(TypeModel type) {
        return new Type<T>(type);
    }

    public static implicit operator TypeRef(Type<T> type) {
        return type.Ref;
    }
}
