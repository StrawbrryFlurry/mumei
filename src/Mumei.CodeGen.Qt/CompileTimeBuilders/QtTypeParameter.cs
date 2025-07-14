using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public sealed class QtTypeParameter : IQtType {
    public Type TypeOf { get; }

    public string Name { get; }

    public static implicit operator QtTypeParameter(string name) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        throw new NotImplementedException();
    }
}

public readonly struct QtTypeParameterCollection(
    QtTypeParameter[]? typeParameters
) : IQtComponent {
    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (typeParameters is null) {
            return;
        }

        writer.Write("<");
        for (var i = 0; i < typeParameters.Length; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            writer.Write(typeParameters[i].Name);
        }

        writer.Write(">");
    }
}