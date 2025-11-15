using System.Collections;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct AttributeFragment(TypeInfoFragment type, AttributeArgumentListFragment argumentList, string? target = null) : IRenderFragment {
    public AttributeArgumentListFragment ArgumentList { get; } = argumentList;

    public static AttributeFragment Create(
        TypeInfoFragment type,
        AttributeArgumentListFragment argumentList = default
    ) {
        return new AttributeFragment(type, argumentList);
    }

    public static AttributeFragment Intercept(
        InterceptableLocation location
    ) {
        var interceptableLocationType = new TypeInfoFragment("global::System.Runtime.CompilerServices.InterceptsLocationAttribute");
        AttributeArgumentListFragment.Builder builder = [
            new PositionalArgumentFragment(location.Version.ToString()),
            new PositionalArgumentFragment("\"" + location.Data + "\"")
        ];
        return new AttributeFragment(interceptableLocationType, builder);
    }

    public static AttributeFragment Create(
        TypeInfoFragment type,
        AttributeArgumentListFragment.Builder arguments = default
    ) {
        return new AttributeFragment(type, arguments.ToAttributeArgumentList());
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text("[");

        if (target is not null) {
            renderTree.Text($"{target}:");
        }

        renderTree.Interpolate($"{type.FullName}{ArgumentList}]");
    }
}

public readonly struct AttributeArgumentListFragment(
    ImmutableArray<PositionalArgumentFragment> positionalArguments,
    ImmutableArray<NamedArgumentFragment> namedArguments,
    ImmutableArray<AttributePropertyArgumentFragment> propertyArguments
) : IRenderFragment {
    public ImmutableArray<PositionalArgumentFragment> PositionalArguments => positionalArguments.EnsureInitialized();
    public ImmutableArray<NamedArgumentFragment> NamedArguments => namedArguments.EnsureInitialized();
    public ImmutableArray<AttributePropertyArgumentFragment> PropertyArguments => propertyArguments.EnsureInitialized();

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

    public ref struct Builder : IEnumerable<PositionalArgumentFragment>, IEnumerable<NamedArgumentFragment>, IEnumerable<AttributePropertyArgumentFragment> {
        private ArrayBuilder<PositionalArgumentFragment> _arguments;
        private ArrayBuilder<NamedArgumentFragment> _namedArguments;
        private ArrayBuilder<AttributePropertyArgumentFragment> _propertyArguments;

        IEnumerator<AttributePropertyArgumentFragment> IEnumerable<AttributePropertyArgumentFragment>.GetEnumerator() {
            throw new NotSupportedException();
        }

        IEnumerator<NamedArgumentFragment> IEnumerable<NamedArgumentFragment>.GetEnumerator() {
            throw new NotSupportedException();
        }

        IEnumerator<PositionalArgumentFragment> IEnumerable<PositionalArgumentFragment>.GetEnumerator() {
            throw new NotSupportedException();
        }

        public IEnumerator GetEnumerator() {
            throw new NotSupportedException();
        }

        public void Add(PositionalArgumentFragment argument) {
            _arguments.Add(argument);
        }

        public void Add(NamedArgumentFragment namedArgument) {
            _namedArguments.Add(namedArgument);
        }

        public void Add(AttributePropertyArgumentFragment propertyArgument) {
            _propertyArguments.Add(propertyArgument);
        }

        public void Add(ExpressionFragment.InterpolatedStringHandler value) {
            _arguments.Add(new PositionalArgumentFragment(new ExpressionFragment(value.GetValue())));
        }

        public AttributeArgumentListFragment ToAttributeArgumentList() {
            return new AttributeArgumentListFragment(
                _arguments.ToImmutableArrayAndFree(),
                _namedArguments.ToImmutableArrayAndFree(),
                _propertyArguments.ToImmutableArrayAndFree()
            );
        }

        public static implicit operator AttributeArgumentListFragment(Builder builder) {
            return builder.ToAttributeArgumentList();
        }
    }
}

public readonly struct AttributePropertyArgumentFragment(ExpressionFragment name, ExpressionFragment value) : IRenderFragment {
    public static implicit operator AttributePropertyArgumentFragment((ExpressionFragment Name, ExpressionFragment Value) keyValuePair) {
        return new AttributePropertyArgumentFragment(keyValuePair.Name, keyValuePair.Value);
    }

    public static implicit operator AttributePropertyArgumentFragment(KeyValuePair<ExpressionFragment, ExpressionFragment> keyValuePair) {
        return new AttributePropertyArgumentFragment(keyValuePair.Key, keyValuePair.Value);
    }

    public static AttributePropertyArgumentFragment Create(ExpressionFragment name, ExpressionFragment value) {
        return new AttributePropertyArgumentFragment(name, value);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"{name} = {value}");
    }
}