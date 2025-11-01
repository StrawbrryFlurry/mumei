using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

[CollectionBuilder(typeof(SynthesizedTypeParameterList), nameof(Initialize))]
public readonly struct SynthesizedTypeParameterList(
    ImmutableArray<SynthesizedTypeParameter> typeParameters
) : IEnumerable<SynthesizedTypeParameter> {
    public RenderNode<ImmutableArray<SynthesizedTypeParameter>> List => new(typeParameters, static (renderTree, typeParameters) => {
        if (typeParameters.IsEmpty) {
            return;
        }

        renderTree.Text("<");
        renderTree.SeparatedList(typeParameters.AsSpan());
        renderTree.Text(">");
    });

    public RenderNode<ImmutableArray<SynthesizedTypeParameter>> Constraints => new(typeParameters, static (renderTree, typeParameters) => {
        if (typeParameters.Length == 0) {
            return;
        }

        foreach (var typeParameter in typeParameters) {
            if (typeParameter.Constraints.IsEmpty) {
                continue;
            }

            renderTree.Interpolate($"where {typeParameter.Name} : ");
            var constraints = typeParameter.Constraints.OrderBy(x => x).ToArray();
            renderTree.SeparatedList(constraints);
        }
    });

    public static SynthesizedTypeParameterList Initialize(ReadOnlySpan<SynthesizedTypeParameter> items) {
        return new SynthesizedTypeParameterList([..items]);
    }

    IEnumerator<SynthesizedTypeParameter> IEnumerable<SynthesizedTypeParameter>.GetEnumerator() {
        return ((IEnumerable<SynthesizedTypeParameter>) typeParameters).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<SynthesizedTypeParameter>) typeParameters).GetEnumerator();
    }
}

public readonly struct SynthesizedTypeParameter(string name, ImmutableArray<SynthesizedTypeParameter.Constraint> constraints) : IRenderNode {
    public string Name { get; } = name;
    public ImmutableArray<Constraint> Constraints { get; } = constraints;

    public void Render(IRenderTreeBuilder renderTree) { }

    public readonly struct Constraint(SynthesizedTypeInfo typeInfo) : IRenderNode, IEquatable<Constraint>, IComparable<Constraint> {
        public SynthesizedTypeInfo TypeInfo { get; } = typeInfo;

        public void Render(IRenderTreeBuilder renderTree) {
            renderTree.Node(TypeInfo.FullName);
        }

        public static Constraint Class => new(new SynthesizedTypeInfo("class"));
        public static Constraint Struct => new(new SynthesizedTypeInfo("struct"));
        public static Constraint New => new(new SynthesizedTypeInfo("new()"));
        public static Constraint NotNull => new(new SynthesizedTypeInfo("notnull"));
        public static Constraint Unmanaged => new(new SynthesizedTypeInfo("unmanaged"));
        public static Constraint AllowsRefStruct => new(new SynthesizedTypeInfo("allows ref struct"));

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
    }

}