using Mumei.CodeGen.Qt.Tests.SyntaxTreeAsExpressionReplacement;

namespace Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.RoslynAsExpressionReplacement;

public interface IRoslynExpressionReceivable<TExpression> where TExpression : Delegate {
    public void Invoke(TExpression expression);
    public void ReceiveExpression(RoslynExpression<TExpression> expression);
}