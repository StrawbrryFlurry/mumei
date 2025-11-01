using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;

internal readonly struct InvocationIntermediateNode(SemanticModel sm, IInvocationOperation operation, InvocationExpressionSyntax invocation) : IEquatable<InvocationIntermediateNode> {
    public IInvocationOperation Operation { get; } = operation;
    public SemanticModel SemanticModel { get; } = sm;
    public InvocationExpressionSyntax Invocation { get; } = invocation;

    public IntermediateMethodInfo MethodInfo => new(SemanticModel, Operation.TargetMethod);

    public bool Equals(InvocationIntermediateNode other) {
        return Invocation.GetText().Equals(other.Invocation.GetText());
    }

    public override bool Equals(object? obj) {
        return obj is InvocationIntermediateNode other && Equals(other);
    }

    public override int GetHashCode() {
        return Invocation.GetText().GetHashCode();
    }

    public InterceptInvocationIntermediateNode<Unit> AsIntercept() {
        return new InterceptInvocationIntermediateNode<Unit>(
            SemanticModel,
            Operation,
            Invocation,
            SemanticModel.GetInterceptableLocation(Invocation) ?? throw new InvalidOperationException("Invocation is not interceptable."),
            Unit.Value
        );
    }
}

internal readonly struct InterceptInvocationIntermediateNode<TState>(
    SemanticModel sm,
    IInvocationOperation operation,
    InvocationExpressionSyntax invocation,
    InterceptableLocation location,
    TState state
) : IEquatable<InterceptInvocationIntermediateNode<TState>> {
    public TState State { get; } = state;

    public InterceptableLocation Location { get; } = location;
    public InvocationExpressionSyntax Invocation { get; } = invocation;
    public IInvocationOperation Operation { get; } = operation;
    public SemanticModel SemanticModel { get; } = sm;

    public bool Equals(InterceptInvocationIntermediateNode<TState> other) {
        return Location.Equals(other.Location);
    }

    public override bool Equals(object? obj) {
        return obj is InterceptInvocationIntermediateNode<TState> other && Equals(other);
    }

    public override int GetHashCode() {
        return Location.GetHashCode();
    }

    public static implicit operator bool(InterceptInvocationIntermediateNode<TState> node) {
        return node.Location is not null;
    }

    public static implicit operator InterceptInvocationIntermediateNode<TState>(IntermediateNode.NoneNode _) {
        return default;
    }

    public InterceptInvocationIntermediateNode<TNewState> WithState<TNewState>(TNewState newState) {
        return new InterceptInvocationIntermediateNode<TNewState>(
            SemanticModel,
            Operation,
            Invocation,
            Location,
            newState
        );
    }
}

public readonly struct IntermediateMethodInfo(SemanticModel semanticModel, IMethodSymbol methodSymbol) {
    public bool IsDeclaredIn<T>() {
        var declaredIn = methodSymbol.ContainingType;
        var targetType = semanticModel.Compilation.GetTypeByMetadataName(typeof(T).FullName!);
        return SymbolEqualityComparer.Default.Equals(declaredIn, targetType);
    }
}

public readonly struct IntermediateTypeInfo(SemanticModel semanticModel, ITypeSymbol typeSymbol) { }

public readonly struct IntermediateNode {
    public static NoneNode None => new();

    public readonly struct NoneNode { }
}