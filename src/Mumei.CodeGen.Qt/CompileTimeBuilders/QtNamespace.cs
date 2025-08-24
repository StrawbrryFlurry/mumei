using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

public readonly struct QtNamespace(
    string name,
    QtCollection<IQtTemplateBindable> declarations
) : ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteLine($"namespace {name} {{");
        writer.Indent();
        foreach (var declaration in declarations) {
            writer.Write(declaration);
            writer.WriteLine();
        }

        writer.Dedent();
        writer.Write("}");
    }

    public QtNamespace WithDeclarations(QtCollection<IQtTemplateBindable> newDeclarations) {
        return new QtNamespace(name, newDeclarations);
    }

    public QtNamespace AddDeclaration(IQtTemplateBindable declaration) {
        return new QtNamespace(name, declarations.Add(declaration));
    }
}