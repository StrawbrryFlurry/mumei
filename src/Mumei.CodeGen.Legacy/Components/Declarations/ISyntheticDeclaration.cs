namespace Mumei.CodeGen.Legacy.Components;

public interface ISyntheticDeclaration {
    public SyntheticIdentifier Name { get; }
    public ISyntheticDeclaration? Parent { get; }
}