using System.Collections.Immutable;

namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct ArgumentListFragment(
    ImmutableArray<PositionalArgumentFragment> positionalArguments,
    ImmutableArray<NamedArgumentFragment> namedArguments
) : IRenderFragment {
    public static readonly ArgumentListFragment Empty = new();

    public static ArgumentListFragment Create(
        ImmutableArray<PositionalArgumentFragment> positionalArguments = default,
        ImmutableArray<NamedArgumentFragment> namedArguments = default
    ) {
        return new ArgumentListFragment(positionalArguments, namedArguments);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        if (positionalArguments.IsEmpty && namedArguments.IsEmpty) {
            return;
        }

        renderTree.Text("(");

        renderTree.SeparatedList(positionalArguments.AsSpan());

        if (!positionalArguments.IsEmpty && !namedArguments.IsEmpty) {
            renderTree.Text(", ");
        }
        renderTree.SeparatedList(namedArguments.AsSpan());

        renderTree.Text(")");
    }
}

public readonly struct PositionalArgumentFragment(ExpressionFragment value) : IRenderFragment {
    public static implicit operator PositionalArgumentFragment(ExpressionFragment value) {
        return new PositionalArgumentFragment(value);
    }

    public static PositionalArgumentFragment Create(ExpressionFragment value) {
        return new PositionalArgumentFragment(value);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Node(value);
    }
}

public readonly struct NamedArgumentFragment(ExpressionFragment name, ExpressionFragment value) : IRenderFragment {
    public static implicit operator NamedArgumentFragment((ExpressionFragment Name, ExpressionFragment Value) keyValuePair) {
        return new NamedArgumentFragment(keyValuePair.Name, keyValuePair.Value);
    }

    public static implicit operator NamedArgumentFragment(KeyValuePair<ExpressionFragment, ExpressionFragment> keyValuePair) {
        return new NamedArgumentFragment(keyValuePair.Key, keyValuePair.Value);
    }

    public static NamedArgumentFragment Create(ExpressionFragment name, ExpressionFragment value) {
        return new NamedArgumentFragment(name, value);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"{name}: {value}");
    }
}