using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtExpression : IQtTemplateBindable {
    private readonly string? _expression;
    private readonly IQtTemplateBindable? _dynamicExpression;

    private QtExpression(string? expression, IQtTemplateBindable? o) {
        _expression = expression;
        _dynamicExpression = o;
    }

    public static QtExpression ForExpression(string expression) {
        return new QtExpression(expression, null);
    }

    public static QtExpression ForBindable(IQtTemplateBindable dynamicExpression) {
        return new QtExpression(null, dynamicExpression);
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (_expression is not null) {
            writer.Write(_expression);
            return;
        }

        if (_dynamicExpression is null) {
            throw new ArgumentNullException(nameof(_dynamicExpression));
        }

        writer.Write(_dynamicExpression);
    }
}