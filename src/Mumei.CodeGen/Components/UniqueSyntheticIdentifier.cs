namespace Mumei.CodeGen.Components;

internal sealed class UniqueSyntheticIdentifier(
    object scope,
    string identifier
) : ISyntheticIdentifier {
    public string Resolve(IIdentifierResolver resolver) {
        return resolver.MakeUnique(scope, identifier);
    }
}