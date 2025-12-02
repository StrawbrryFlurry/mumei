using System.Collections.Concurrent;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.RoslynCodeProviders;

internal sealed class RoslynIdentifierScopeProvider : ISyntheticIdentifierScopeProvider {
    private readonly ConcurrentDictionary<SyntheticIdentifier, ISyntheticIdentifierScope> _identifierScopes = new();

    public ISyntheticIdentifierScope GetDeclarationScope(ISyntheticDeclaration scope) {
        return _identifierScopes.GetOrAdd(scope.Name, _ => new SyntheticIdentifierScope());
    }
}