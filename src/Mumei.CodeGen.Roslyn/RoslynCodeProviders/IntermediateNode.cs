using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.RoslynCodeProviders;

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
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        var isDefaultInstance = node.Location is null;
        return !isDefaultInstance;
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
    public INamedTypeSymbol ContainingType => methodSymbol.ContainingType;

    public bool IsDeclaredIn<T>() {
        var declaredIn = methodSymbol.ContainingType;
        var targetTypeName = typeof(T).FullName!;
        if (typeof(T).IsGenericType) {
            targetTypeName = targetTypeName[..(targetTypeName.IndexOf('`') + 2)];
        }

        var targetType = semanticModel.Compilation.GetTypeByMetadataName(targetTypeName);
        return SymbolEqualityComparer.Default.Equals(declaredIn, targetType);
    }

    public bool IsImplementedFrom<T>() {
        var declaredIn = methodSymbol.OriginalDefinition;
        var targetTypeName = typeof(T).FullName!;
        if (typeof(T).IsGenericType) {
            targetTypeName = targetTypeName[..(targetTypeName.IndexOf('`') + 2)];
        }

        var targetType = semanticModel.Compilation.GetTypeByMetadataName(targetTypeName);
        return SymbolEqualityComparer.Default.Equals(declaredIn, targetType);
    }

    public bool IsImplementedFromAnyConstructedFormOf<T>() {
        return IsImplementedFromAnyConstructedFormOf(typeof(T));
    }

    public bool IsImplementedFromAnyConstructedFormOf(Type t) {
        var targetTypeName = t.FullName!;
        if (t.IsGenericType) {
            targetTypeName = targetTypeName[..(targetTypeName.IndexOf('`') + 2)];
        }

        var unconstructedTargetType = semanticModel.Compilation.GetTypeByMetadataName(targetTypeName);

        if (methodSymbol.OverriddenMethod is not null && TryConstructTypeFromDeclaration(unconstructedTargetType, methodSymbol.OverriddenMethod.ContainingType, out var constructedTypeFromOverride)) {
            return SymbolEqualityComparer.Default.Equals(methodSymbol.OverriddenMethod.ContainingType, constructedTypeFromOverride);
        }

        var interfaces = ContainingType.AllInterfaces;
        foreach (var ifaceImpl in interfaces) {
            if (!TryConstructTypeFromDeclaration(unconstructedTargetType, ifaceImpl, out var constructedType)) {
                continue;
            }

            foreach (var member in ifaceImpl.GetMembers(methodSymbol.Name)) {
                var implementedMember = ContainingType.FindImplementationForInterfaceMember(member);
                if (SymbolEqualityComparer.Default.Equals(implementedMember, methodSymbol)) {
                    return true;
                }
            }
        }

        // TODO:
        if (methodSymbol.ExplicitInterfaceImplementations.Any()) { }

        return true;
    }

    public bool IsDeclaredInAnyConstructedFormOf<T>() {
        var declaredIn = methodSymbol.ContainingType;

        var targetTypeName = typeof(T).FullName!;
        if (typeof(T).IsGenericType) {
            targetTypeName = targetTypeName[..(targetTypeName.IndexOf('`') + 2)];
        }

        var targetType = semanticModel.Compilation.GetTypeByMetadataName(targetTypeName);
        if (!TryConstructTypeFromDeclaration(targetType, declaredIn, out var constructedType)) {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(declaredIn, constructedType);
    }

    private bool TryConstructTypeFromDeclaration(INamedTypeSymbol? targetType, INamedTypeSymbol declarationSymbol, [NotNullWhen(true)] out INamedTypeSymbol? constructedType) {
        if (targetType is null || !targetType.IsGenericType || declarationSymbol.TypeArguments.Length != targetType.TypeArguments.Length) {
            constructedType = null;
            return false;
        }

        constructedType = targetType.Construct(declarationSymbol.TypeArguments, declarationSymbol.TypeArgumentNullableAnnotations);
        return true;
    }

    public IntermediateTypeInfo ReturnTypeInfo => new(semanticModel, methodSymbol.ReturnType);

    public ImmutableArray<ITypeSymbol> TypeArguments => methodSymbol.TypeArguments;
}

public readonly struct IntermediateTypeInfo(SemanticModel semanticModel, ITypeSymbol typeSymbol) { }

public readonly struct IntermediateNode {
    public static NoneNode None => new();

    public readonly struct NoneNode { }
}

public interface IIntermediateNode<TSelf> where TSelf : struct, IIntermediateNode<TSelf> {
    public bool HasValue { get; }
    public IEqualityComparer<TSelf> EqualityComparer { get; }
}