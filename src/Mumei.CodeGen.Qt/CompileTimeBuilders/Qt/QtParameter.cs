using System.Collections.Immutable;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtParameter(
    string name,
    IQtType type,
    ParameterModifier modifiers = ParameterModifier.None,
    IQtCompileTimeValue? defaultValue = null
) : IQtTemplateBindable {
    public string Name { get; } = name;
    public IQtType Type { get; } = type;

    public ParameterModifier Modifiers { get; } = modifiers;

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteFormatted($"{Modifiers}{Type:g} {Name}");
    }
}

public readonly struct QtParameterList(
    QtParameter[] parameters
) : IQtTemplateBindable {
    internal static QtParameterList Builder(int capacity) {
        return new QtParameterList(new QtParameter[capacity]);
    }

    internal QtParameter this[int index] {
        set => parameters[index] = value;
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        for (var i = 0; i < parameters.Length; i++) {
            var parameter = parameters[i];
            if (i > 0) {
                writer.Write(", ");
            }

            parameter.WriteSyntax(ref writer, format);
        }
    }
}