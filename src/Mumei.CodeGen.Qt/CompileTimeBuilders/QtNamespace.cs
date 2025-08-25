using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

public readonly struct QtNamespace(
    string name,
    QtCollection<IQtTypeDeclaration> declarations
) : ISyntaxRepresentable {
    private readonly List<ISourceFileFeature> _features = [];

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

    public static QtNamespace FromGeneratorAssemblyName(string assemblyScopedName, QtCollection<IQtTypeDeclaration> declarations = default) {
        string fullName;
        var assemblyName = QtCompilationScope.Active.Compilation.AssemblyName;
        if (assemblyName is null) {
            if (assemblyScopedName.StartsWith(".")) {
                fullName = assemblyScopedName[1..];
            } else {
                fullName = assemblyScopedName;
            }
        } else {
            fullName = $"{assemblyName}.{assemblyScopedName}";
        }

        return new QtNamespace(fullName, declarations);
    }

    public QtNamespace WithDeclarations(QtCollection<IQtTypeDeclaration> newDeclarations) {
        return new QtNamespace(name, newDeclarations);
    }

    public QtNamespace AddDeclaration(IQtTypeDeclaration declaration) {
        return new QtNamespace(name, declarations.Add(declaration));
    }
}