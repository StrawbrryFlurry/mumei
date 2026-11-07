using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen;

/// <summary>
/// Full semantic access to the symbol a <see cref="SymbolSource{T,TSymbol}"/> item was created from.
/// Only available inside <c>Where</c>/<c>Select</c> of a symbol source: anything returned from there
/// must be an equatable snapshot so the generator stays incremental.
/// </summary>
public sealed class SemanticContext<TSymbol> where TSymbol : ISymbol {
    private IOperation? _operation;

    internal SemanticContext(TSymbol symbol, SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken) {
        Symbol = symbol;
        Node = node;
        SemanticModel = semanticModel;
        CancellationToken = cancellationToken;
    }

    public TSymbol Symbol { get; }
    public SyntaxNode Node { get; }
    public SemanticModel SemanticModel { get; }
    public Compilation Compilation => SemanticModel.Compilation;
    public CancellationToken CancellationToken { get; }

    /// <summary>The operation of <see cref="Node"/>, e.g. the <see cref="Microsoft.CodeAnalysis.Operations.IInvocationOperation"/> of a call.</summary>
    public IOperation? Operation => _operation ??= SemanticModel.GetOperation(Node, CancellationToken);
}
