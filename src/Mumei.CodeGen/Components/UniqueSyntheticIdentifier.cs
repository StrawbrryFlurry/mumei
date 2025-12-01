namespace Mumei.CodeGen.Components;

internal sealed class UniqueSyntheticIdentifier(
    IdentifierScope scope,
    string identifier
) : ISyntheticIdentifier {
    public string Resolve(IIdentifierResolver resolver) {
        // Assuming scope is class
        // If the user creates a class with the unique name "Foo"
        // and then emits that class once with that initial name
        // later, if the user tries to emit another class using the same
        // initial name "Foo", but changes it later down the line before emission
        // we don't have to track them back to the same unique identifier, because
        // at this point they are already unique due to the name change.

        return resolver.MakeUnique(scope, identifier);
    }
}