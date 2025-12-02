namespace Mumei.CodeGen.Components;

public interface ISyntheticIdentifierScopeProvider {
    public ISyntheticIdentifierScope GetDeclarationScope(ISyntheticDeclaration scope);
}