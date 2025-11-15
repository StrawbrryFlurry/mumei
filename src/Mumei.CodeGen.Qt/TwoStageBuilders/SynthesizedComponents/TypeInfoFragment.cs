using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct TypeInfoFragment : IEquatable<TypeInfoFragment> {
    public string QualifiedTypeName { get; }
    public string Name { get; }

    /// <summary>
    /// Determines wether the type info represents a non-runtime keyword type e.g. var, class, unmanaged etc.
    /// </summary>
    internal bool IsNonRuntimeKeyword { get; }

    public RenderFragment<TypeInfoFragment> FullName => new(this, static (renderTree, node) => {
        renderTree.Text(node.QualifiedTypeName);
    });

    public RenderFragment<TypeInfoFragment> TypeOf => new(this, static (renderTree, node) => {
        renderTree.Interpolate($"typeof({node.QualifiedTypeName})");
    });

    public static TypeInfoFragment ForKeyword(string keyword) {
        return new TypeInfoFragment(keyword, true);
    }

    public TypeInfoFragment(ITypeSymbol type) : this(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), false) { }
    public TypeInfoFragment(Type type) : this(RuntimeTypeSerializer.GetTypeFullName(type), false) { }
    public TypeInfoFragment(string qualifiedTypeName) : this(qualifiedTypeName, false) { }

    internal TypeInfoFragment(string qualifiedTypeName, bool isNonRuntimeKeyword) {
        QualifiedTypeName = qualifiedTypeName;
        IsNonRuntimeKeyword = isNonRuntimeKeyword;

        Name = qualifiedTypeName.Contains('.')
            ? qualifiedTypeName[(qualifiedTypeName.LastIndexOf('.') + 1)..]
            : qualifiedTypeName;
    }

    public static implicit operator TypeInfoFragment(Type type) {
        return new TypeInfoFragment(type);
    }

    public static bool operator ==(TypeInfoFragment left, TypeInfoFragment right) {
        return left.Equals(right);
    }

    public static bool operator !=(TypeInfoFragment left, TypeInfoFragment right) {
        return !(left == right);
    }

    public bool Equals(TypeInfoFragment other) {
        return QualifiedTypeName == other.QualifiedTypeName;
    }

    public override bool Equals(object? obj) {
        return obj is TypeInfoFragment other && Equals(other);
    }

    public override int GetHashCode() {
        return QualifiedTypeName.GetHashCode();
    }
}