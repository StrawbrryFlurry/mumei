using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen;

/// <summary>
/// An equatable snapshot of a type declaration including its members.
/// Renders as the type's globally qualified name.
/// </summary>
public sealed record TypeModel : IRenderFragment {
    public required TypeRef Type { get; init; }
    public Modifiers Modifiers { get; init; }
    public TypeRef? BaseType { get; init; }

    /// <summary>All interfaces implemented by the type, including inherited ones.</summary>
    public EquatableArray<TypeRef> Interfaces { get; init; } = EquatableArray<TypeRef>.Empty;

    public EquatableArray<AttributeModel> Attributes { get; init; } = EquatableArray<AttributeModel>.Empty;
    public EquatableArray<PropertyModel> Properties { get; init; } = EquatableArray<PropertyModel>.Empty;
    public EquatableArray<FieldModel> Fields { get; init; } = EquatableArray<FieldModel>.Empty;
    public EquatableArray<MethodModel> Methods { get; init; } = EquatableArray<MethodModel>.Empty;
    public EquatableArray<MethodModel> Constructors { get; init; } = EquatableArray<MethodModel>.Empty;

    /// <summary>The types this type is nested in, outermost first.</summary>
    public EquatableArray<TypeRef> ContainingTypes { get; init; } = EquatableArray<TypeRef>.Empty;

    public SourceLocation? Location { get; init; }

    public string Name => Type.Name;
    public string Namespace => Type.Namespace;
    public string FullName => Type.FullName;

    /// <summary>Whether the type implements the interface with the given metadata name, e.g. <c>"MyLib.IHandler`2"</c>.</summary>
    public bool Implements(string interfaceMetadataName) {
        return Interface(interfaceMetadataName) is not null;
    }

    /// <summary>The first implemented interface with the given metadata name, including its type arguments.</summary>
    public TypeRef? Interface(string interfaceMetadataName) {
        foreach (var @interface in Interfaces) {
            if (@interface.Is(interfaceMetadataName)) {
                return @interface;
            }
        }

        return null;
    }

    public bool HasAttribute(string attributeMetadataName) {
        return Attribute(attributeMetadataName) is not null;
    }

    public AttributeModel? Attribute(string attributeMetadataName) {
        foreach (var attribute in Attributes) {
            if (attribute.Type.Is(attributeMetadataName)) {
                return attribute;
            }
        }

        return null;
    }

    public static implicit operator TypeRef(TypeModel model) {
        return model.Type;
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(Type.FullName);
    }

    public override string ToString() {
        return Type.FullName;
    }
}
