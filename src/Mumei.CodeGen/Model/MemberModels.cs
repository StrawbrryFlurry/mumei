using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen;

/// <summary>An equatable snapshot of a property. Renders as the property name.</summary>
public sealed record PropertyModel(
    string Name,
    TypeRef Type,
    Modifiers Modifiers,
    bool HasGetter,
    bool HasSetter,
    bool IsInitOnly
) : IRenderFragment {
    public EquatableArray<AttributeModel> Attributes { get; init; } = EquatableArray<AttributeModel>.Empty;

    public bool IsRequired => (Modifiers & Modifiers.Required) != 0;
    public bool IsStatic => (Modifiers & Modifiers.Static) != 0;

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(CSharpIdentifier.Escape(Name));
    }

    public override string ToString() {
        return Name;
    }
}

/// <summary>An equatable snapshot of a field. Renders as the field name.</summary>
public sealed record FieldModel(string Name, TypeRef Type, Modifiers Modifiers) : IRenderFragment {
    public EquatableArray<AttributeModel> Attributes { get; init; } = EquatableArray<AttributeModel>.Empty;

    /// <summary>The value of a <c>const</c> field.</summary>
    public object? ConstantValue { get; init; }

    public bool IsStatic => (Modifiers & Modifiers.Static) != 0;

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(CSharpIdentifier.Escape(Name));
    }

    public override string ToString() {
        return Name;
    }
}

/// <summary>An equatable snapshot of a method parameter. Renders as the parameter name.</summary>
public sealed record ParameterModel(string Name, TypeRef Type, ParameterModifiers Modifiers = ParameterModifiers.None)
    : IRenderFragment {
    public bool HasDefaultValue { get; init; }
    public object? DefaultValue { get; init; }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(CSharpIdentifier.Escape(Name));
    }

    public override string ToString() {
        return Name;
    }
}

/// <summary>An equatable snapshot of a method. Renders as the method name.</summary>
public sealed record MethodModel(
    string Name,
    TypeRef ContainingType,
    TypeRef ReturnType,
    EquatableArray<ParameterModel> Parameters,
    Modifiers Modifiers
) : IRenderFragment {
    /// <summary>The type arguments of a constructed generic method, or its type parameters.</summary>
    public EquatableArray<TypeRef> TypeArguments { get; init; } = EquatableArray<TypeRef>.Empty;

    public EquatableArray<AttributeModel> Attributes { get; init; } = EquatableArray<AttributeModel>.Empty;

    /// <summary>
    /// Whether this is an extension method. Extension methods are always described in their static form,
    /// so the receiver is the first parameter.
    /// </summary>
    public bool IsExtension { get; init; }

    public bool ReturnsVoid => ReturnType.IsVoid;
    public bool IsStatic => (Modifiers & Modifiers.Static) != 0;
    public bool IsGeneric => !TypeArguments.IsEmpty;

    public bool HasAttribute(string attributeMetadataName) {
        foreach (var attribute in Attributes) {
            if (attribute.Type.Is(attributeMetadataName)) {
                return true;
            }
        }

        return false;
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(CSharpIdentifier.Escape(Name));
    }

    public override string ToString() {
        return $"{ContainingType}.{Name}";
    }
}

/// <summary>
/// An equatable snapshot of an attribute application. Arguments are primitives, strings,
/// <see cref="TypeRef"/> (for <c>typeof</c>) or <see cref="EquatableArray{T}"/> of <c>object?</c> (for arrays).
/// </summary>
public sealed record AttributeModel(
    TypeRef Type,
    EquatableArray<object?> Arguments,
    EquatableArray<NamedArgument> NamedArguments
) {
    public T? Argument<T>(int index) {
        return index < Arguments.Length && Arguments[index] is T value ? value : default;
    }

    public T? Argument<T>(string name) {
        foreach (var argument in NamedArguments) {
            if (argument.Name == name && argument.Value is T value) {
                return value;
            }
        }

        return default;
    }
}

public readonly record struct NamedArgument(string Name, object? Value);
