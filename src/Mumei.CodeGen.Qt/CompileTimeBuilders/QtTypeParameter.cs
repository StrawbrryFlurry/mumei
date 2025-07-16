using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public readonly struct QtTypeParameter : IQtType {
    public Type TypeOf { get; }
    public string Name { get; }

    public string Constraint { get; }

    public static implicit operator QtTypeParameter(string name) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        throw new NotImplementedException();
    }
}

public readonly struct QtTypeParameterList(
    QtTypeParameter[]? typeParameters
) : IQtTemplateBindable {
    public ConstraintImpl Constraints { get; } = new();

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
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

    public readonly struct ConstraintImpl : IQtTemplateBindable {
        public bool IsEmpty => true;

        public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
            throw new NotImplementedException();
        }
    }
}