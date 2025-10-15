using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

public readonly struct QtNamespace(
    string name,
    QtCollection<IQtTypeDeclaration> declarations
) : IRenderNode {
    private readonly List<ISourceFileFeature> _features = [];

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text($"namespace {name}");
        renderTree.Text(" ");
        renderTree.StartCodeBlock();

        for (var i = 0; i < declarations.Count; i++) {
            var declaration = declarations[i];
            renderTree.Node(declaration);
            if (i < declarations.Count - 1) {
                renderTree.NewLine();
            }
        }

        renderTree.EndCodeBlock();
    }

    public static QtNamespace FromGeneratorAssemblyName(string assemblyScopedName, params QtCollection<IQtTypeDeclaration> declarations) {
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