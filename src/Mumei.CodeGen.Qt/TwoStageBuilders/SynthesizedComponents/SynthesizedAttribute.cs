using System.Collections;
using System.Collections.Immutable;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedAttribute(SynthesizedTypeInfo type, SynthesizedAttributeArgumentList argumentList) : IRenderNode {
    public SynthesizedAttributeArgumentList ArgumentList { get; } = argumentList;

    public static SynthesizedAttribute Create(
        SynthesizedTypeInfo type,
        SynthesizedAttributeArgumentList argumentList = default
    ) {
        return new SynthesizedAttribute(type, argumentList);
    }

    public static SynthesizedAttribute Create(
        SynthesizedTypeInfo type,
        SynthesizedAttributeArgumentList.Builder arguments = default
    ) {
        return new SynthesizedAttribute(type, arguments.ToAttributeArgumentList());
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"[{type.FullName}{ArgumentList}]");
    }
}

public readonly struct SynthesizedAttributeArgumentList(
    ImmutableArray<SynthesizedPositionalArgument> positionalArguments,
    ImmutableArray<SynthesizedNamedArgument> namedArguments,
    ImmutableArray<SynthesizedAttributePropertyArgument> propertyArguments
) : IRenderNode {
    public ImmutableArray<SynthesizedPositionalArgument> PositionalArguments => positionalArguments.EnsureInitialized();
    public ImmutableArray<SynthesizedNamedArgument> NamedArguments => namedArguments.EnsureInitialized();
    public ImmutableArray<SynthesizedAttributePropertyArgument> PropertyArguments => propertyArguments.EnsureInitialized();

    public void Render(IRenderTreeBuilder renderTree) {
        if (PositionalArguments.IsEmpty && NamedArguments.IsEmpty && PropertyArguments.IsEmpty) {
            return;
        }

        renderTree.Text("(");

        renderTree.SeparatedList(PositionalArguments.AsSpan());
        if (!PositionalArguments.IsEmpty && (!NamedArguments.IsEmpty || !PropertyArguments.IsEmpty)) {
            renderTree.Text(", ");
        }
        renderTree.SeparatedList(NamedArguments.AsSpan());

        if (!NamedArguments.IsEmpty && !PropertyArguments.IsEmpty) {
            renderTree.Text(", ");
        }
        renderTree.SeparatedList(PropertyArguments.AsSpan());

        renderTree.Text(")");
    }

    public ref struct Builder : IEnumerable<SynthesizedPositionalArgument>, IEnumerable<SynthesizedNamedArgument>, IEnumerable<SynthesizedAttributePropertyArgument> {
        private ArrayBuilder<SynthesizedPositionalArgument> _arguments;
        private ArrayBuilder<SynthesizedNamedArgument> _namedArguments;
        private ArrayBuilder<SynthesizedAttributePropertyArgument> _propertyArguments;

        IEnumerator<SynthesizedAttributePropertyArgument> IEnumerable<SynthesizedAttributePropertyArgument>.GetEnumerator() {
            throw new NotSupportedException();
        }

        IEnumerator<SynthesizedNamedArgument> IEnumerable<SynthesizedNamedArgument>.GetEnumerator() {
            throw new NotSupportedException();
        }

        IEnumerator<SynthesizedPositionalArgument> IEnumerable<SynthesizedPositionalArgument>.GetEnumerator() {
            throw new NotSupportedException();
        }

        public IEnumerator GetEnumerator() {
            throw new NotSupportedException();
        }

        public void Add(SynthesizedPositionalArgument argument) {
            _arguments.Add(argument);
        }

        public void Add(SynthesizedNamedArgument namedArgument) {
            _namedArguments.Add(namedArgument);
        }

        public void Add(SynthesizedAttributePropertyArgument propertyArgument) {
            _propertyArguments.Add(propertyArgument);
        }

        public void Add(SynthesizedExpression.InterpolatedStringHandler value) {
            _arguments.Add(new SynthesizedPositionalArgument(new SynthesizedExpression(value.GetValue())));
        }

        public SynthesizedAttributeArgumentList ToAttributeArgumentList() {
            return new SynthesizedAttributeArgumentList(
                _arguments.ToImmutableArrayAndFree(),
                _namedArguments.ToImmutableArrayAndFree(),
                _propertyArguments.ToImmutableArrayAndFree()
            );
        }

        public static implicit operator SynthesizedAttributeArgumentList(Builder builder) {
            return builder.ToAttributeArgumentList();
        }
    }
}

public readonly struct SynthesizedAttributePropertyArgument(SynthesizedExpression name, SynthesizedExpression value) : IRenderNode {
    public static implicit operator SynthesizedAttributePropertyArgument((SynthesizedExpression Name, SynthesizedExpression Value) keyValuePair) {
        return new SynthesizedAttributePropertyArgument(keyValuePair.Name, keyValuePair.Value);
    }

    public static implicit operator SynthesizedAttributePropertyArgument(KeyValuePair<SynthesizedExpression, SynthesizedExpression> keyValuePair) {
        return new SynthesizedAttributePropertyArgument(keyValuePair.Key, keyValuePair.Value);
    }

    public static SynthesizedAttributePropertyArgument Create(SynthesizedExpression name, SynthesizedExpression value) {
        return new SynthesizedAttributePropertyArgument(name, value);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"{name} = {value}");
    }
}