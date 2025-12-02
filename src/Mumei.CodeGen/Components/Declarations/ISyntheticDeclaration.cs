namespace Mumei.CodeGen.Components;

public interface ISyntheticDeclaration {
    public SyntheticIdentifier Name { get; }
    public ISyntheticDeclaration? Parent { get; }
}