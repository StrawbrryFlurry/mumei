using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Qt.Tests.SyntaxTreeAsExpressionReplacement;

public sealed class RoslynExpression<TExpression> {
    public required ParameterSyntax[] Parameters { get; init; }
    public required StatementSyntax[] Statements { get; init; }
}