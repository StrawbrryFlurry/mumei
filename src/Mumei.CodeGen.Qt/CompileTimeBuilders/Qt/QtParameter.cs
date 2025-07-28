using System.Collections.Immutable;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtParameter : IQtTemplateBindable {
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
}

public readonly struct QtParameterList(
    QtParameter[] parameters
) : IQtTemplateBindable, IQtMemoryAccessor<QtParameter> {
    public int Count => parameters.Length;

    internal QtParameter[] Parameters => parameters;
    public Memory<QtParameter> Memory => Parameters.AsMemory();

    internal static QtParameterList Builder(int capacity) {
        return new QtParameterList(new QtParameter[capacity]);
    }

    internal QtParameter this[int index] {
        get => parameters[index];
        set => parameters[index] = value;
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        for (var i = 0; i < parameters.Length; i++) {
            var parameter = parameters[i];
            if (i > 0) {
                writer.Write(", ");
            }

            writer.Write(parameter);
        }
    }

    public bool TryGetThisParameter(out QtParameter parameter) {
        var first = parameters.Length > 0 ? parameters[0] : default;
        if (first.Attributes.HasFlag(ParameterAttributes.This)) {
            parameter = first;
            return true;
        }

        parameter = default;
        return false;
    }
}