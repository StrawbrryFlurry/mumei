namespace Mumei.CodeGen;

/// <summary>
/// Builds an interceptor for a single <see cref="CallSite"/>. The signature is derived from the intercepted method;
/// only the body needs to be provided:
/// <code>
/// i.Body(b => {
///     b.Line($"Console.WriteLine({call.Method.Name:literal});");
///     b.Line($"return {i.Proceed()};");
/// });
/// </code>
/// </summary>
public sealed class InterceptorBuilder {
    public const string DefaultNamespace = "Mumei.Generated";

    private readonly MethodBuilder _method;

    internal InterceptorBuilder(CallSite call) {
        Call = call;
        ClassName = CSharpIdentifier.Sanitize(call.Method.Name) + "Interceptor";

        var method = call.Method;
        _method = new MethodBuilder("Intercept", method.ReturnType, Modifiers.Public | Modifiers.Static, isConstructor: false);
        _method.Attribute(call.InterceptLocation.ToAttributeSyntax());

        var parameters = method.Parameters.AsSpan();
        if (method.IsExtension) {
            var receiver = parameters[0];
            Receiver = _method.Parameter(receiver.Type, receiver.Name, receiver.Modifiers | ParameterModifiers.This);
            parameters = parameters[1..];
        } else if (!method.IsStatic) {
            var receiverModifiers = ParameterModifiers.This | (method.ContainingType.IsValueType ? ParameterModifiers.Ref : 0);
            Receiver = _method.Parameter(method.ContainingType, "this", receiverModifiers);
        }

        var arguments = new List<ParameterModel>(parameters.Length);
        foreach (var parameter in parameters) {
            arguments.Add(_method.Parameter(parameter.Type, parameter.Name, parameter.Modifiers));
        }

        Arguments = arguments.ToEquatableArray();
    }

    public CallSite Call { get; }

    /// <summary>The name of the (file-local) class containing the interceptor.</summary>
    public string ClassName { get; set; }

    /// <summary>The namespace of the interceptor. It needs to be listed in the <c>InterceptorsNamespaces</c> MSBuild property.</summary>
    public string Namespace { get; set; } = DefaultNamespace;

    /// <summary>The receiver of the call (<c>@this</c>, or the first argument of an extension method), if any.</summary>
    public ParameterModel? Receiver { get; }

    /// <summary>The parameters of the interceptor matching the arguments of the call, excluding the receiver.</summary>
    public EquatableArray<ParameterModel> Arguments { get; }

    /// <summary>
    /// Why the call can't be intercepted, e.g. because it involves type parameters of the calling method.
    /// </summary>
    public string? UnsupportedReason {
        get {
            var method = Call.Method;
            var involvesTypeParameters = method.ContainingType.ContainsTypeParameters
                                         || method.ReturnType.ContainsTypeParameters
                                         || method.TypeArguments.Any(t => t.ContainsTypeParameters)
                                         || method.Parameters.Any(p => p.Type.ContainsTypeParameters);
            return involvesTypeParameters
                ? $"Intercepting '{method}' at {Call.Location} is not supported because the call involves generic type parameters."
                : null;
        }
    }

    public ParameterModel Arg(string name) {
        foreach (var argument in Arguments) {
            if (argument.Name == name) {
                return argument;
            }
        }

        throw new ArgumentException($"'{Call.Method}' has no parameter named '{name}'.", nameof(name));
    }

    public ParameterModel Arg(int index) {
        return Arguments[index];
    }

    /// <summary>The original invocation, passing all arguments through, e.g. <c>@this.Method(a, b)</c>.</summary>
    public Code Proceed() {
        return Proceed(Arguments.Select(a => (Code)$"{a.Modifiers.ToArgumentPrefix()}{a}").ToArray());
    }

    /// <summary>The original invocation with replaced arguments.</summary>
    public Code Proceed(params Code[] arguments) {
        var method = Call.Method;
        var typeArguments = method.IsGeneric ? $"<{string.Join(", ", method.TypeArguments)}>" : "";
        var argumentList = string.Join(", ", arguments.Select(a => a.ToString()));
        var name = CSharpIdentifier.Escape(method.Name);

        if (method.IsExtension) {
            var receiver = CSharpIdentifier.Escape(Receiver!.Name);
            var allArguments = arguments.Length == 0 ? receiver : $"{receiver}, {argumentList}";
            return $"{method.ContainingType}.{name}{typeArguments}({allArguments})";
        }

        return method.IsStatic
            ? $"{method.ContainingType}.{name}{typeArguments}({argumentList})"
            : $"@this.{name}{typeArguments}({argumentList})";
    }

    /// <summary>Like <see cref="Proceed()"/>, but as a statement: <c>return @this.M(a);</c> or <c>@this.M(a);</c> for void methods.</summary>
    public Code ReturnProceed() {
        return Call.Method.ReturnsVoid ? $"{Proceed()};" : $"return {Proceed()};";
    }

    /// <inheritdoc cref="ReturnProceed()"/>
    public Code ReturnProceed(params Code[] arguments) {
        return Call.Method.ReturnsVoid ? $"{Proceed(arguments)};" : $"return {Proceed(arguments)};";
    }

    public InterceptorBuilder Attribute(Code attribute) {
        _method.Attribute(attribute);
        return this;
    }

    public InterceptorBuilder Body(Code body) {
        _method.Body(body);
        return this;
    }

    public InterceptorBuilder Body(Action<CodeBuilder> body) {
        _method.Body(body);
        return this;
    }

    public InterceptorBuilder ExpressionBody(Code expression) {
        _method.ExpressionBody(expression);
        return this;
    }

    internal SourceFileBuilder ToSourceFile() {
        var file = new SourceFileBuilder();
        var interceptors = file.Namespace(Namespace).Class(ClassName, Modifiers.File | Modifiers.Static);
        interceptors.AddMember(_method);

        file.Namespace("System.Runtime.CompilerServices").Member(
            """
            [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
            file sealed class InterceptsLocationAttribute : global::System.Attribute {
                public InterceptsLocationAttribute(int version, string data) { }
            }
            """
        );

        return file;
    }

    internal string HintName => $"{CSharpIdentifier.Sanitize(ClassName)}.{StableHash.Compute(Call.InterceptLocation.Data):x16}.g.cs";
}
