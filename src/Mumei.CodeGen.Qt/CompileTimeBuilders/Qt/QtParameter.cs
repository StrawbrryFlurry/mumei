using System.Collections;
using System.Runtime.CompilerServices;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtParameter : IQtTemplateBindable, IRenderNode {
    public required string Name { get; init; }
    public required IQtType Type { get; init; }
    public required IQtCompileTimeValue? DefaultValue { get; init; }
    public required ParameterAttributes Attributes { get; init; }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteFormatted($"{Attributes}{Type:g} {Name}");
        if (DefaultValue is not null) {
            writer.Write(" = ");
            writer.Write(DefaultValue);
        }
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"{Attributes.List}{Type.FullName} {Name}");
        if (DefaultValue is not null) {
            renderTree.Text(" = ");
            // renderTree.Node(DefaultValue);
        }
    }
}

[CollectionBuilder(typeof(QtParameterList), nameof(Initialize))]
public readonly struct QtParameterList(
    QtCollection<QtParameter> parameters
) : IQtTemplateBindable, IRenderNode, IQtMemoryAccessor<QtParameter>, IEnumerable<QtParameter> {
    public int Count => parameters.Count;

    internal QtCollection<QtParameter> Parameters => parameters;
    public Memory<QtParameter> Memory => Parameters.Memory;

    internal static QtParameterList Builder(int capacity) {
        return new QtParameterList(QtCollection<QtParameter>.Create(capacity));
    }

    public static QtParameterList Initialize(ReadOnlySpan<QtParameter> items) {
        return new QtParameterList([..items]);
    }

    internal QtParameter this[int index] {
        get => parameters[index];
        set => parameters[index] = value;
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        for (var i = 0; i < parameters.Count; i++) {
            var parameter = parameters[i];
            if (i > 0) {
                writer.Write(", ");
            }

            writer.Write(parameter);
        }
    }

    public bool TryGetThisParameter(out QtParameter parameter) {
        var first = parameters.Count > 0 ? parameters[0] : default;
        if (first.Attributes.HasFlag(ParameterAttributes.This)) {
            parameter = first;
            return true;
        }

        parameter = default;
        return false;
    }

    public void Render(IRenderTreeBuilder renderer) {
        renderer.SeparatedList(parameters.Span);
    }

    public Span<QtParameter>.Enumerator GetEnumerator() {
        return Parameters.GetEnumerator();
    }

    IEnumerator<QtParameter> IEnumerable<QtParameter>.GetEnumerator() {
        return ((IEnumerable<QtParameter>) Parameters).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<QtParameter>) this).GetEnumerator();
    }
}