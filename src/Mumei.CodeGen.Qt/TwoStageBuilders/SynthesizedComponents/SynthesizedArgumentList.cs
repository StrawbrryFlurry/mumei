using System.Collections.Immutable;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedArgumentList(
    ImmutableArray<SynthesizedPositionalArgument> positionalArguments,
    ImmutableArray<SynthesizedNamedArgument> namedArguments
) : IRenderNode {
    public static readonly SynthesizedArgumentList Empty = new();

    public static SynthesizedArgumentList Create(
        ImmutableArray<SynthesizedPositionalArgument> positionalArguments = default,
        ImmutableArray<SynthesizedNamedArgument> namedArguments = default
    ) {
        return new SynthesizedArgumentList(positionalArguments, namedArguments);
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

public readonly struct SynthesizedPositionalArgument(SynthesizedExpression value) : IRenderNode {
    public static implicit operator SynthesizedPositionalArgument(SynthesizedExpression value) {
        return new SynthesizedPositionalArgument(value);
    }

    public static SynthesizedPositionalArgument Create(SynthesizedExpression value) {
        return new SynthesizedPositionalArgument(value);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Node(value);
    }
}

public readonly struct SynthesizedNamedArgument(SynthesizedExpression name, SynthesizedExpression value) : IRenderNode {
    public static implicit operator SynthesizedNamedArgument((SynthesizedExpression Name, SynthesizedExpression Value) keyValuePair) {
        return new SynthesizedNamedArgument(keyValuePair.Name, keyValuePair.Value);
    }

    public static implicit operator SynthesizedNamedArgument(KeyValuePair<SynthesizedExpression, SynthesizedExpression> keyValuePair) {
        return new SynthesizedNamedArgument(keyValuePair.Key, keyValuePair.Value);
    }

    public static SynthesizedNamedArgument Create(SynthesizedExpression name, SynthesizedExpression value) {
        return new SynthesizedNamedArgument(name, value);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"{name}: {value}");
    }
}