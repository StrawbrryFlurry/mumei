using System.Collections.Concurrent;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.RoslynCodeProviders;

internal sealed class RoslynIdentifierScopeProvider : ISyntheticIdentifierScopeProvider {
    private readonly ConcurrentDictionary<SyntheticIdentifier, ISyntheticIdentifierScope> _identifierScopes = new();

    public ISyntheticIdentifierScope GetDeclarationScope(ISyntheticDeclaration scope) {
        // TODO: We should represent all known scopes as a tree based on the root declaration
        // this scope is part of to make identifier names more predictable.
        // For now this is sufficient and still guarantees uniqueness.
        return _identifierScopes.GetOrAdd(scope.Name, _ => new SyntheticIdentifierScope());
    }
}