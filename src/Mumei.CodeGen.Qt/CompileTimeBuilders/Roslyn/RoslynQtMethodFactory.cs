using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground.Roslyn;

// ReSharper disable once InconsistentNaming
public readonly ref struct __DynamicallyBoundSourceCode { }

internal readonly ref struct RoslynQtMethodFactory(
    QtCompilationScope scope
) {
    public QtMethod<CompileTimeUnknown> CreateProxyMethodForInvocation(
        InvocationExpressionSyntax invocationToProxy,
        __DynamicallyBoundSourceCode sourceCode,
        QtDeclarationPtr<QtMethodCore> declPtr
    ) {
        var method = new QtMethod<CompileTimeUnknown>(
            "QtProxy__" + ((MemberAccessExpressionSyntax)invocationToProxy.Expression).Name.Identifier.Text,
            AccessModifier.PublicStatic,
            QtType.ForRuntimeType(typeof(bool)),
            new QtTypeParameterList(),
            new QtParameterList([
                new QtParameter("first", QtType.ForRuntimeType<IEnumerable<int>>(), ParameterModifier.This),
                new QtParameter("second", QtType.ForRuntimeType<IEnumerable<int>>())
            ]),
            null!,
            declPtr
        );

        return method;
    }
}