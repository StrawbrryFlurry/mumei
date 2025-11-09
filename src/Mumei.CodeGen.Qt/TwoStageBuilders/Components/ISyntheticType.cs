using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticType { }
public interface ISyntheticMember { }

public interface ISyntheticTypeInfo<T> {
    public T New(params object[] args);
    public T New(Func<T> constructorExpression);
}

internal sealed class RuntimeSyntheticType(Type t) : ISyntheticType { }

internal sealed class QtSyntheticTypeInfo<T> : ISyntheticTypeInfo<T> {
    public T New(params object[] args) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public T New(Func<T> constructorExpression) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    [MacroBinding]
    public SyntaxNode Bind__New(MacroBindingContext ctx) {
        var invocation = ctx.AssertNode<InvocationExpressionSyntax>();
        var operation = ctx.AssertOperation<IInvocationOperation>();

        if (operation.Arguments is [{ Value: IDelegateCreationOperation } newExpression]) {
            if (newExpression.Value.Syntax is not LambdaExpressionSyntax lambdaSyntax) {
                throw new MacroBindingException(ctx, "Expected delegate creation operation to have a lambda expression syntax.");
            }
        }

        throw new NotImplementedException();
    }
}

/// <summary>
/// Declares the method as a macro binding for the synthetic compilation system.
/// Components participating in the macro system are expected to expose a method
/// `Bind__{Method/Property/Field}Name/Self` that takes a <see cref="MacroBindingContext"/> parameter
/// and returns a <see cref="SyntaxNode"/> representing the bound syntax for that member or itself at
/// the macro callsite.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MacroBindingAttribute : Attribute;

public sealed class MacroBindingException(MacroBindingContext context, string message) : Exception(message) {
    public MacroBindingContext Context { get; } = context;
}

public readonly struct MacroBindingContext {
    public SyntaxNode SyntaxNode { get; }
    public StatementSyntax EnclosingStatement { get; }
    public SyntheticCompilation Compilation { get; }
    public IOperation Operation { get; }

    public TNode AssertNode<TNode>() where TNode : SyntaxNode {
        if (SyntaxNode is not TNode node) {
            throw new MacroBindingException(this, $"Expected syntax node of type {typeof(TNode).FullName}, but got {SyntaxNode.GetType().FullName}");
        }

        return node;
    }

    public TOperation AssertOperation<TOperation>() where TOperation : IOperation {
        if (Operation is not TOperation operation) {
            throw new MacroBindingException(this, $"Expected operation of type {typeof(TOperation).FullName}, but got {Operation.GetType().FullName}");
        }

        return operation;
    }
}