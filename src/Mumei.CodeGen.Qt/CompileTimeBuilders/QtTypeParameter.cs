using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

public readonly struct QtTypeParameter {
    public Type TypeOf { get; }
    public required string Name { get; init; }
    public required string? Constraint { get; init; }
}

public readonly struct QtTypeParameterList(
    QtCollection<QtTypeParameter> typeParameters
) : IQtTemplateBindable, IRenderNode {
    public ConstraintImpl Constraints { get; } = new();

    public bool IsEmpty => typeParameters.Count == 0;
    public int Count => typeParameters.Count;

    public static QtTypeParameterList Builder(int capacity) {
        return new QtTypeParameterList(QtCollection<QtTypeParameter>.Create(capacity));
    }

    internal QtTypeParameter this[int idx] {
        set => typeParameters[idx] = value;
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (IsEmpty) {
            return;
        }

        writer.Write("<");
        for (var i = 0; i < typeParameters.Count; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            writer.Write(typeParameters[i].Name);
        }

        writer.Write(">");
    }

    public readonly struct ConstraintImpl : IQtTemplateBindable, IRenderNode {
        public bool IsEmpty => true;

        public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
            throw new NotImplementedException();
        }

        public void Render(IRenderer renderer) {
            throw new NotImplementedException();
        }
    }

    public void Render(IRenderer renderer) {
        throw new NotImplementedException();
    }
}