using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen;

/// <summary>Common base of everything that can be declared inside a type.</summary>
public abstract class MemberBuilder : IRenderFragment {
    private protected readonly List<Code> AttributeList = [];

    private protected MemberBuilder(string name) {
        Name = name;
    }

    public string Name { get; }

    /// <summary>Renders the member's declaration.</summary>
    internal abstract void RenderDeclaration(IRenderTreeBuilder tree);

    /// <summary>Renders as the member name, so builders can be used as references in <see cref="Code"/>.</summary>
    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(CSharpIdentifier.Escape(Name));
    }

    public override string ToString() {
        return Name;
    }

    private protected void RenderAttributes(IRenderTreeBuilder tree) {
        foreach (var attribute in AttributeList) {
            tree.Line($"[{attribute}]");
        }
    }

    private protected static string Prefix(Modifiers modifiers) {
        var syntax = modifiers.ToSyntax();
        return syntax.Length == 0 ? "" : syntax + " ";
    }
}

public sealed class FieldBuilder : MemberBuilder {
    internal FieldBuilder(TypeRef type, string name, Modifiers modifiers, Code initializer) : base(name) {
        Type = type;
        Modifiers = modifiers;
        Initializer = initializer;
    }

    public TypeRef Type { get; }
    public Modifiers Modifiers { get; set; }
    public Code Initializer { get; set; }

    public FieldBuilder Attribute(Code attribute) {
        AttributeList.Add(attribute);
        return this;
    }

    internal override void RenderDeclaration(IRenderTreeBuilder tree) {
        RenderAttributes(tree);
        tree.Text($"{Prefix(Modifiers)}{Type.FullName} {CSharpIdentifier.Escape(Name)}");
        if (!Initializer.IsEmpty) {
            tree.Text(" = ");
            tree.Text(Initializer.ToString());
        }

        tree.Line(";");
    }
}

public sealed class PropertyBuilder : MemberBuilder {
    private string _accessors = "get; set;";
    private Code _getter;

    internal PropertyBuilder(TypeRef type, string name, Modifiers modifiers) : base(name) {
        Type = type;
        Modifiers = modifiers;
    }

    public TypeRef Type { get; }
    public Modifiers Modifiers { get; set; }
    public Code Initializer { get; private set; }

    /// <summary>Makes this a computed property: <c>Name => expression;</c></summary>
    public PropertyBuilder Get(Code expression) {
        _getter = expression;
        return this;
    }

    /// <summary><c>{ get; }</c></summary>
    public PropertyBuilder GetOnly() {
        return Accessors("get;");
    }

    /// <summary><c>{ get; init; }</c></summary>
    public PropertyBuilder InitOnly() {
        return Accessors("get; init;");
    }

    /// <summary>Sets the accessor list verbatim, e.g. <c>"get; private set;"</c>.</summary>
    public PropertyBuilder Accessors(string accessors) {
        _accessors = accessors;
        return this;
    }

    public PropertyBuilder WithInitializer(Code initializer) {
        Initializer = initializer;
        return this;
    }

    public PropertyBuilder Attribute(Code attribute) {
        AttributeList.Add(attribute);
        return this;
    }

    internal override void RenderDeclaration(IRenderTreeBuilder tree) {
        RenderAttributes(tree);
        tree.Text($"{Prefix(Modifiers)}{Type.FullName} {CSharpIdentifier.Escape(Name)}");
        if (!_getter.IsEmpty) {
            tree.Line($" => {_getter};");
            return;
        }

        tree.Text($" {{ {_accessors} }}");
        if (!Initializer.IsEmpty) {
            tree.Text($" = {Initializer};");
        }

        tree.NewLine();
    }
}

public sealed class MethodBuilder : MemberBuilder {
    private readonly List<ParameterModel> _parameters = [];
    private readonly List<(TypeRef Parameter, Code Constraint)> _typeParameters = [];
    private string? _body;
    private Code _expressionBody;
    private Code _constructorInitializer;

    internal MethodBuilder(string name, TypeRef returnType, Modifiers modifiers, bool isConstructor) : base(name) {
        ReturnType = returnType;
        Modifiers = modifiers;
        IsConstructor = isConstructor;
    }

    public TypeRef ReturnType { get; }
    public Modifiers Modifiers { get; set; }
    public bool IsConstructor { get; }
    public IReadOnlyList<ParameterModel> Parameters => _parameters;

    /// <summary>Adds a parameter and returns a reference to it for use in the body.</summary>
    public ParameterModel Parameter<T>(string name, ParameterModifiers modifiers = ParameterModifiers.None) {
        return Parameter(TypeRef.Of<T>(), name, modifiers);
    }

    public ParameterModel Parameter(TypeRef type, string name, ParameterModifiers modifiers = ParameterModifiers.None) {
        var parameter = new ParameterModel(name, type, modifiers);
        _parameters.Add(parameter);
        return parameter;
    }

    /// <summary>Adds a parameter, keeping the builder chain: <c>.Parameter&lt;int&gt;("x", out var x).Body($"return {x};")</c></summary>
    public MethodBuilder Parameter<T>(string name, out ParameterModel parameter, ParameterModifiers modifiers = ParameterModifiers.None) {
        parameter = Parameter(TypeRef.Of<T>(), name, modifiers);
        return this;
    }

    public MethodBuilder Parameter(TypeRef type, string name, out ParameterModel parameter, ParameterModifiers modifiers = ParameterModifiers.None) {
        parameter = Parameter(type, name, modifiers);
        return this;
    }

    /// <summary>Adds a parameter with a default value.</summary>
    public ParameterModel OptionalParameter(TypeRef type, string name, object? defaultValue) {
        var parameter = new ParameterModel(name, type) { HasDefaultValue = true, DefaultValue = defaultValue };
        _parameters.Add(parameter);
        return parameter;
    }

    /// <summary>Declares a method type parameter and returns a reference to it.</summary>
    public TypeRef TypeParameter(string name, Code constraint = default) {
        var parameter = TypeRef.TypeParameter(name);
        _typeParameters.Add((parameter, constraint));
        return parameter;
    }

    public MethodBuilder Attribute(Code attribute) {
        AttributeList.Add(attribute);
        return this;
    }

    public MethodBuilder Body(Code body) {
        _body = body.ToString();
        return this;
    }

    public MethodBuilder Body(Action<CodeBuilder> body) {
        var builder = new CodeBuilder();
        body(builder);
        _body = builder.ToString();
        return this;
    }

    /// <summary><c>=> expression;</c></summary>
    public MethodBuilder ExpressionBody(Code expression) {
        _expressionBody = expression;
        return this;
    }

    /// <summary>The constructor initializer, e.g. <c>$"base({name})"</c>.</summary>
    public MethodBuilder Initializer(Code initializer) {
        _constructorInitializer = initializer;
        return this;
    }

    internal override void RenderDeclaration(IRenderTreeBuilder tree) {
        RenderAttributes(tree);
        tree.Text(Prefix(Modifiers));
        if (!IsConstructor) {
            tree.Text(ReturnType.FullName);
            tree.Text(" ");
        }

        tree.Text(CSharpIdentifier.Escape(Name));
        if (_typeParameters.Count > 0) {
            tree.Text($"<{string.Join(", ", _typeParameters.Select(p => p.Parameter.Name))}>");
        }

        tree.Text("(");
        for (var i = 0; i < _parameters.Count; i++) {
            if (i > 0) {
                tree.Text(", ");
            }

            RenderParameter(tree, _parameters[i]);
        }

        tree.Text(")");

        if (!_constructorInitializer.IsEmpty) {
            tree.Text($" : {_constructorInitializer}");
        }

        foreach (var (parameter, constraint) in _typeParameters) {
            if (!constraint.IsEmpty) {
                tree.Text($" where {parameter.Name} : {constraint}");
            }
        }

        if (!_expressionBody.IsEmpty) {
            tree.Line($" => {_expressionBody};");
            return;
        }

        var hasNoBody = (Modifiers & (Modifiers.Abstract | Modifiers.Extern)) != 0
                        || ((Modifiers & Modifiers.Partial) != 0 && _body is null);
        if (hasNoBody) {
            tree.Line(";");
            return;
        }

        if (string.IsNullOrEmpty(_body)) {
            tree.Line(" { }");
            return;
        }

        tree.Text(" ");
        tree.StartCodeBlock();
        tree.Lines(_body!);
        tree.EndCodeBlock();
    }

    internal static void RenderParameter(IRenderTreeBuilder tree, ParameterModel parameter) {
        var modifiers = parameter.Modifiers.ToSyntax();
        if (modifiers.Length > 0) {
            tree.Text(modifiers);
            tree.Text(" ");
        }

        tree.Text($"{parameter.Type.FullName} {CSharpIdentifier.Escape(parameter.Name)}");
        if (parameter.HasDefaultValue) {
            tree.Text($" = {CSharpLiteral.Format(parameter.DefaultValue)}");
        }
    }
}

/// <summary>A member declared from verbatim code.</summary>
internal sealed class RawMemberBuilder(Code code) : MemberBuilder("") {
    internal override void RenderDeclaration(IRenderTreeBuilder tree) {
        tree.Lines(code.ToString());
    }
}
