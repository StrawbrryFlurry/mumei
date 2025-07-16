namespace Mumei.CodeGen.Qt.Output;

// public interface ISyntaxRepresentable<TRepresenter> where TRepresenter : struct, ISyntaxRepresenter {
//     public TRepresenter RepresentSyntax();
// }

public interface ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter;
}