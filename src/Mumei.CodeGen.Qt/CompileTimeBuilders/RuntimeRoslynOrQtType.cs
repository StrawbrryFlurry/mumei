using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

internal readonly struct RuntimeRoslynOrQtType : IQtTemplateBindable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }

    public static implicit operator RuntimeRoslynOrQtType(Type type) {
        return new RuntimeRoslynOrQtType();
    }

    public static implicit operator RuntimeRoslynOrQtType(QtType type) {
        return new RuntimeRoslynOrQtType();
    }
}