namespace Mumei.CodeGen.Legacy.Components;

public interface ISyntheticIdentifierScopeProvider {
    public ISyntheticIdentifierScope GetDeclarationScope(ISyntheticDeclaration scope);
}