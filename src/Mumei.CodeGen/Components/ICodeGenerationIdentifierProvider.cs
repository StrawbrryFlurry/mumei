namespace Mumei.CodeGen.Components;

public interface ICodeGenerationIdentifierProvider {
    public IdentifierScope GlobalScope { get; }

    public IdentifierScope CreateScope(IdentifierScope parentScope, string name);
    public IdentifierScope CreateScope(string name);
}