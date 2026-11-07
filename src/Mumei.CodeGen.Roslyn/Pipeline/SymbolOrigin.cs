using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen;

/// <summary>Where the items of a <see cref="SymbolSource{T,TSymbol}"/> come from, e.g. types with an attribute.</summary>
internal abstract class SymbolOrigin<TSymbol> where TSymbol : ISymbol {
    public abstract IncrementalValuesProvider<T> Create<T>(GeneratorContext gen, Func<SemanticContext<TSymbol>, Maybe<T>> transform);

    protected static IncrementalValuesProvider<T> Unwrap<T>(IncrementalValuesProvider<Maybe<T>> provider) {
        return provider.Where(x => x.HasValue).Select((x, _) => x.Value!);
    }

    /// <summary>Partial declarations are reported once per part; only keep the first one.</summary>
    protected static bool IsFirstDeclaration(ISymbol symbol, SyntaxNode node) {
        var references = symbol.DeclaringSyntaxReferences;
        return references.Length <= 1
               || (references[0].SyntaxTree == node.SyntaxTree && references[0].Span == node.Span);
    }
}

internal sealed class AttributeOrigin<TSymbol>(string attributeMetadataName, Func<SyntaxNode, bool> nodePredicate)
    : SymbolOrigin<TSymbol> where TSymbol : ISymbol {
    public override IncrementalValuesProvider<T> Create<T>(GeneratorContext gen, Func<SemanticContext<TSymbol>, Maybe<T>> transform) {
        return Unwrap(gen.Roslyn.SyntaxProvider.ForAttributeWithMetadataName(
            attributeMetadataName,
            (node, _) => nodePredicate(node),
            (context, cancellationToken) => context.TargetSymbol is TSymbol symbol && IsFirstDeclaration(symbol, context.TargetNode)
                ? transform(new SemanticContext<TSymbol>(symbol, context.TargetNode, context.SemanticModel, cancellationToken))
                : default
        ));
    }
}

internal sealed class DeclarationOrigin<TSymbol>(Func<SyntaxNode, bool> nodePredicate)
    : SymbolOrigin<TSymbol> where TSymbol : ISymbol {
    public override IncrementalValuesProvider<T> Create<T>(GeneratorContext gen, Func<SemanticContext<TSymbol>, Maybe<T>> transform) {
        return Unwrap(gen.Roslyn.SyntaxProvider.CreateSyntaxProvider(
            (node, _) => nodePredicate(node),
            (context, cancellationToken) =>
                context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) is TSymbol symbol
                && IsFirstDeclaration(symbol, context.Node)
                    ? transform(new SemanticContext<TSymbol>(symbol, context.Node, context.SemanticModel, cancellationToken))
                    : default
        ));
    }
}

/// <summary>Invocations, optionally pre-filtered by the invoked method name.</summary>
internal sealed class CallOrigin(string? methodName) : SymbolOrigin<IMethodSymbol> {
    public override IncrementalValuesProvider<T> Create<T>(GeneratorContext gen, Func<SemanticContext<IMethodSymbol>, Maybe<T>> transform) {
        return Unwrap(gen.Roslyn.SyntaxProvider.CreateSyntaxProvider(
            (node, _) => node is InvocationExpressionSyntax invocation
                         && (methodName is null
                             || SymbolSnapshots.GetInvokedNameSyntax(invocation.Expression)?.Identifier.ValueText == methodName),
            (context, cancellationToken) =>
                context.SemanticModel.GetSymbolInfo(context.Node, cancellationToken).Symbol is IMethodSymbol method
                    ? transform(new SemanticContext<IMethodSymbol>(method, context.Node, context.SemanticModel, cancellationToken))
                    : default
        ));
    }
}
