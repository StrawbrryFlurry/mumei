using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Rendering.CSharp;

[CollectionBuilder(typeof(TypeParameterListFragment), nameof(Initialize))]
public readonly struct TypeParameterListFragment(
    ImmutableArray<TypeParameterFragment> typeParameters
) : IEnumerable<TypeParameterFragment> {
    public static TypeParameterListFragment Empty => new([]);

    public ImmutableArray<TypeParameterFragment> TypeParameters => typeParameters.EnsureInitialized();

    public RenderFragment<ImmutableArray<TypeParameterFragment>> List => new(TypeParameters, static (renderTree, typeParameters) => {
        if (typeParameters.IsEmpty) {
            return;
        }

        renderTree.Text("<");
        renderTree.SeparatedList(typeParameters.AsSpan());
        renderTree.Text(">");
    });

    public RenderFragment<ImmutableArray<TypeParameterFragment>> Constraints => new(TypeParameters, static (renderTree, typeParameters) => {
        if (typeParameters.Length == 0) {
            return;
        }

        foreach (var typeParameter in typeParameters) {
            if (typeParameter.Constraints.IsEmpty) {
                continue;
            }

            renderTree.Interpolate($" where {typeParameter.Name} : ");
            var constraints = typeParameter.Constraints.OrderBy(x => x).ToArray();
            renderTree.SeparatedList(constraints);
        }
    });

    public static TypeParameterListFragment Initialize(ReadOnlySpan<TypeParameterFragment> items) {
        return new TypeParameterListFragment([..items]);
    }

    IEnumerator<TypeParameterFragment> IEnumerable<TypeParameterFragment>.GetEnumerator() {
        return ((IEnumerable<TypeParameterFragment>) TypeParameters).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<TypeParameterFragment>) TypeParameters).GetEnumerator();
    }

    public override string ToString() {
        return DebugRenderer.Render(List);
    }
}

public readonly struct TypeParameterFragment(string name, ImmutableArray<TypeParameterFragment.Constraint> constraints) : IRenderFragment {
    public string Name { get; } = name;
    public ImmutableArray<Constraint> Constraints { get; } = constraints;

    public static TypeParameterFragment Create(
        string name,
        out TypeParameterFragment typeParameter,
        params ImmutableArray<Constraint> constraints
    ) {
        typeParameter = Create(name, constraints);
        return typeParameter;
    }

    public static TypeParameterFragment Create(
        string name,
        params ImmutableArray<Constraint> constraints
    ) {
        return new TypeParameterFragment(name, constraints);
    }

    public static implicit operator TypeInfoFragment(TypeParameterFragment typeParameter) {
        return TypeInfoFragment.ForKeyword(typeParameter.Name);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(Name);
    }

    public override string ToString() {
        return DebugRenderer.Render(this);
    }

    public readonly struct Constraint(TypeInfoFragment typeInfo) : IRenderFragment, IEquatable<Constraint>, IComparable<Constraint> {
        public TypeInfoFragment TypeInfo { get; } = typeInfo;

        public void Render(IRenderTreeBuilder renderTree) {
            renderTree.Node(TypeInfo.FullName);
        }

        public static Constraint Class => new(new TypeInfoFragment("class"));
        public static Constraint NullableClass => new(new TypeInfoFragment("class?"));
        public static Constraint Struct => new(new TypeInfoFragment("struct"));
        public static Constraint New => new(new TypeInfoFragment("new()"));
        public static Constraint NotNull => new(new TypeInfoFragment("notnull"));
        public static Constraint Unmanaged => new(new TypeInfoFragment("unmanaged"));
        public static Constraint AllowsRefStruct => new(new TypeInfoFragment("allows ref struct"));

        public static implicit operator Constraint(TypeInfoFragment typeInfo) {
            return new Constraint(typeInfo);
        }

        public static implicit operator Constraint(Type type) {
            return new Constraint(new TypeInfoFragment(type));
        }

        public bool Equals(Constraint other) {
            return other.TypeInfo == TypeInfo;
        }

        public int CompareTo(Constraint other) {
            if (TypeInfo.IsNonRuntimeKeyword) {
                return other.TypeInfo.IsNonRuntimeKeyword ? 0 : -1;
            }

            if (other.TypeInfo.IsNonRuntimeKeyword) {
                return 1;
            }

            return 0;
        }

        public override string ToString() {
            return DebugRenderer.Render(this);
        }
    }
}