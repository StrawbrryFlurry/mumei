using System.Collections.Immutable;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtParameter(
    string name,
    IQtType type,
    ParameterModifier modifiers = ParameterModifier.None
) : IQtTemplateBindable {
    public string Name { get; } = name;
    public IQtType Type { get; } = type;

    public ParameterModifier Modifiers { get; } = modifiers;

    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteFormatted($"{Modifiers}{Type:g} {Name}");
    }
}

public readonly struct QtParameterList(
    ImmutableArray<QtParameter> parameters
) : IQtTemplateBindable {
    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        for (var i = 0; i < parameters.Length; i++) {
            var parameter = parameters[i];
            if (i > 0) {
                writer.Write(", ");
            }

            parameter.WriteSyntax(writer, format);
        }
    }
}