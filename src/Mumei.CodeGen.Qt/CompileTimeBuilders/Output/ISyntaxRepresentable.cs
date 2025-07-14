namespace Mumei.CodeGen.Qt.Output;

// public interface ISyntaxRepresentable<TRepresenter> where TRepresenter : struct, ISyntaxRepresenter {
//     public TRepresenter RepresentSyntax();
// }

public interface ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter;
}