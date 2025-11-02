using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedTypeInfo : IEquatable<SynthesizedTypeInfo> {
    public string QualifiedTypeName { get; }
    public string Name { get; }

    /// <summary>
    /// Determines wether the type info represents a non-runtime keyword type e.g. var, class, unmanaged etc.
    /// </summary>
    internal bool IsNonRuntimeKeyword { get; }

    public RenderNode<SynthesizedTypeInfo> FullName => new(this, static (renderTree, node) => {
        renderTree.Text(node.QualifiedTypeName);
    });

    public RenderNode<SynthesizedTypeInfo> TypeOf => new(this, static (renderTree, node) => {
        renderTree.Interpolate($"typeof({node.QualifiedTypeName})");
    });

    public SynthesizedTypeInfo(Type type) : this(RuntimeTypeSerializer.GetTypeFullName(type), false) { }
    public SynthesizedTypeInfo(string qualifiedTypeName) : this(qualifiedTypeName, false) { }

    internal SynthesizedTypeInfo(string qualifiedTypeName, bool isNonRuntimeKeyword) {
        QualifiedTypeName = qualifiedTypeName;
        IsNonRuntimeKeyword = isNonRuntimeKeyword;

        Name = qualifiedTypeName.Contains('.')
            ? qualifiedTypeName[(qualifiedTypeName.LastIndexOf('.') + 1)..]
            : qualifiedTypeName;
    }

    public static implicit operator SynthesizedTypeInfo(Type type) {
        return new SynthesizedTypeInfo(type);
    }

    public static bool operator ==(SynthesizedTypeInfo left, SynthesizedTypeInfo right) {
        return left.Equals(right);
    }

    public static bool operator !=(SynthesizedTypeInfo left, SynthesizedTypeInfo right) {
        return !(left == right);
    }

    public bool Equals(SynthesizedTypeInfo other) {
        return QualifiedTypeName == other.QualifiedTypeName;
    }

    public override bool Equals(object? obj) {
        return obj is SynthesizedTypeInfo other && Equals(other);
    }

    public override int GetHashCode() {
        return QualifiedTypeName.GetHashCode();
    }
}