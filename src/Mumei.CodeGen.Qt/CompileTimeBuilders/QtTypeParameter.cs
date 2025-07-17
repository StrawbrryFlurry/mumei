using System.Diagnostics.CodeAnalysis;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground;

public readonly struct QtTypeParameter {
    public Type TypeOf { get; }
    public required string Name { get; init; }
    public required string? Constraint { get; init; }
}

public readonly struct QtTypeParameterList(
    QtTypeParameter[] typeParameters
) : IQtTemplateBindable {
    public ConstraintImpl Constraints { get; } = new();

    private readonly QtTypeParameter[]? _typeParameters = typeParameters;

    [MemberNotNullWhen(false, nameof(_typeParameters))]
    public bool IsEmpty => _typeParameters is null || _typeParameters.Length == 0;

    public static QtTypeParameterList Builder(int capacity) {
        return new QtTypeParameterList(new QtTypeParameter[capacity]);
    }

    internal QtTypeParameter this[int idx] {
        set => _typeParameters?[idx] = value;
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (IsEmpty) {
            return;
        }

        writer.Write("<");
        for (var i = 0; i < _typeParameters.Length; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            writer.Write(_typeParameters[i].Name);
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