using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.Roslyn;

internal readonly ref struct RoslynQtComponentFactory(
    QtCompilationScope scope
) {
    private readonly Compilation _compilation = scope.Compilation;

    public IQtCompileTimeValue CompileTimeValue<TValue>(TValue value) {
        if (value is null) {
            return QtCompileTimeValue.Null;
        }

        if (value is IQtCompileTimeValue compileTimeValue) {
            return compileTimeValue;
        }

#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        return QtCompileTimeValue.ForLiteral(value);
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    }

    public QtTypeParameterList TypeParametersOf(
        IMethodSymbol methodSymbol
    ) {
        var typeParameters = methodSymbol.TypeParameters;
        var result = QtTypeParameterList.Builder(typeParameters.Length);
        for (var i = 0; i < typeParameters.Length; i++) {
            var typeParameter = typeParameters[i];
            result[i] = TypeParameter(typeParameter);
        }

        return result;
    }

    public QtTypeParameter TypeParameter(
        ITypeParameterSymbol typeParameter
    ) {
        var name = typeParameter.Name;

        return new QtTypeParameter {
            Name = name,
            Constraint = null
        };
    }

    public QtParameterList InterceptParametersFor(
        IMethodSymbol method
    ) {
        var additionalThisArg = !method.IsStatic ? 1 : 0;
        var result = QtParameterList.Builder(method.Parameters.Length + additionalThisArg);
        var parameters = method.Parameters;

        if (additionalThisArg == 1) {
            result[0] = new QtParameter {
                Name = "this".Obfuscate(),
                Type = Type(method.ReceiverType!),
                DefaultValue = null,
                Attributes = ParameterAttributes.This
            };
        }

        for (var i = 0; i < parameters.Length; i++) {
            var parameter = parameters[i];
            result[i + additionalThisArg] = Parameter(parameter);
        }

        return result;
    }

    public QtParameterList ParametersOf(
        IMethodSymbol method
    ) {
        var additionalThisArg = method.IsExtensionMethod ? 1 : 0;
        var result = QtParameterList.Builder(method.Parameters.Length + additionalThisArg);
        var parameters = method.Parameters;

        if (method.IsExtensionMethod) {
            result[0] = new QtParameter {
                Name = "this".Obfuscate(),
                Type = Type(method.ReceiverType!),
                DefaultValue = null,
                Attributes = ParameterAttributes.This
            };
        }

        for (var i = 0; i < parameters.Length; i++) {
            var parameter = parameters[i];
            result[i + additionalThisArg] = Parameter(parameter);
        }

        return result;
    }

    public QtParameter Parameter(IParameterSymbol parameter) {
        var type = Type(parameter.Type);
        var attributes = ParameterAttributes.None;

        if (parameter.RefKind == RefKind.Ref) {
            attributes |= ParameterAttributes.Ref;
        }
        else if (parameter.RefKind == RefKind.RefReadOnlyParameter) {
            attributes |= ParameterAttributes.Ref | ParameterAttributes.Readonly;
        }
        else if (parameter.RefKind == RefKind.Out) {
            attributes |= ParameterAttributes.Out;
        }
        else if (parameter.RefKind == RefKind.In) {
            attributes |= ParameterAttributes.In;
        }

        if (parameter.IsParams) {
            attributes |= ParameterAttributes.Params;
        }

        if (parameter.IsThis) {
            attributes |= ParameterAttributes.This;
        }

        if (attributes != ParameterAttributes.None) {
            attributes &= ~ParameterAttributes.None;
        }

        var defaultValue = parameter.HasExplicitDefaultValue
            ? CompileTimeValue(parameter.ExplicitDefaultValue)
            : null;

        return new QtParameter {
            Name = parameter.Name.Obfuscate(),
            Type = type,
            DefaultValue = defaultValue,
            Attributes = attributes
        };
    }

    public QtInvocation Invocation(
        InvocationExpressionSyntax invocation
    ) {
        var method = GetSymbol<IMethodSymbol>(invocation);
        var target = InvocationReplayTarget(invocation, method);
        return new QtInvocation {
            Target = target,
            Method = new QtMethodStub {
                Name = method.Name,
                IsThisCall = method.IsExtensionMethod
            },
            TypeArguments = default,
            Arguments = default
        };
    }

    private QtExpression InvocationReplayTarget(
        InvocationExpressionSyntax invocation,
        IMethodSymbol method
    ) {
        if (method.IsStatic || method.IsExtensionMethod) {
            var declaringType = Type(method.ContainingType);
            return QtExpression.ForBindable(declaringType);
        }

        // Missing an invocation target, has to be an implicit call to a non-static instance method.
        if (invocation.Expression is IdentifierNameSyntax or GenericNameSyntax) {
            return QtExpression.ForExpression("this");
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) {
            throw new NotSupportedException("Found a non-static, non-instance method invocation without a valid target expression.");
        }

        return QtExpression.ForExpression(memberAccess.ToString());
    }

    private QtArgumentList ReplayArguments(
        ArgumentListSyntax argumentList
    ) {
        var arguments = QtArgumentList.Builder(argumentList.Arguments.Count);
        foreach (var argument in argumentList.Arguments) { }

        return arguments;
    }

    private TSymbol GetSymbol<TSymbol>(SyntaxNode node) where TSymbol : ISymbol {
        var model = SemanticModel(node);
        var symbol = model.GetSymbolInfo(node).Symbol;
        if (symbol is not TSymbol typedSymbol) {
            throw new InvalidOperationException(
                $"Expected retrieving symbol of type {typeof(TSymbol)} from {node}, but got {symbol?.GetType().ToString() ?? "null"}"
            );
        }

        return typedSymbol;
    }

    private SemanticModel SemanticModel(SyntaxNode node) {
        return _compilation.GetSemanticModel(node.SyntaxTree);
    }

    private IQtType Type(ITypeSymbol type) {
        return QtType.ForRoslynType(type);
    }
}