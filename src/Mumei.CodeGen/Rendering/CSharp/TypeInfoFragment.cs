using Mumei.Common.Internal;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct TypeInfoFragment : IEquatable<TypeInfoFragment> {
    public string QualifiedTypeName { get; }
    public string Name { get; }

    public bool HasNullableAnnotation { get; }

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
        return new TypeInfoFragment(keyword, false, true);
    }

    public static TypeInfoFragment Var => ForKeyword("var");

    public static TypeInfoFragment ConstructGenericType(
        TypeInfoFragment unconstructedType,
        params ReadOnlySpan<TypeInfoFragment> typeArguments
    ) {
        return GenericTypeInfoFragment.Construct(unconstructedType, typeArguments);
    }

    public TypeInfoFragment(Type type) : this(RuntimeTypeSerializer.GetTypeFullName(type), IsNullableWrapper(type), false) { }
    public TypeInfoFragment(string qualifiedTypeName) : this(qualifiedTypeName, qualifiedTypeName.EndsWith("?"), false) { }

    internal TypeInfoFragment(string qualifiedTypeName, bool hasNullableAnnotation, bool isNonRuntimeKeyword) {
        QualifiedTypeName = qualifiedTypeName;
        IsNonRuntimeKeyword = isNonRuntimeKeyword;

        HasNullableAnnotation = hasNullableAnnotation;

        var name = qualifiedTypeName.Contains('.')
            ? qualifiedTypeName.AsSpan()[(qualifiedTypeName.LastIndexOf('.') + 1)..]
            : qualifiedTypeName;

        if (HasNullableAnnotation && name.EndsWith("?")) {
            name = name[..^1];
        }

        Name = name.SequenceEqual(qualifiedTypeName) ? qualifiedTypeName : name.ToString();
    }

    public TypeInfoFragment ToNullable() {
        return HasNullableAnnotation ? this : new TypeInfoFragment(QualifiedTypeName, true, IsNonRuntimeKeyword);
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

    private static bool IsNullableWrapper(Type type) {
        return type.IsValueType && type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public override string ToString() {
        return DebugRenderer.Render(FullName);
    }

    private readonly struct GenericTypeInfoFragment {
        public static TypeInfoFragment Construct(TypeInfoFragment constructableType, ReadOnlySpan<TypeInfoFragment> typeArguments) {
            var nameBuilder = new ArrayBuilder<char>(stackalloc char[ArrayBuilder.InitSize]);
            nameBuilder.AddRange(constructableType.QualifiedTypeName);
            nameBuilder.Add('<');

            for (var i = 0; i < typeArguments.Length; i++) {
                if (i > 0) {
                    nameBuilder.Add(',');
                    nameBuilder.Add(' ');
                }

                nameBuilder.AddRange(typeArguments[i].QualifiedTypeName);
            }

            nameBuilder.Add('>');
            return new TypeInfoFragment(nameBuilder.ToStringAndFree());
        }
    }
}