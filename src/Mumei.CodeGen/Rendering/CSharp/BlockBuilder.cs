namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct BlockBuilder {
    private readonly List<StatementBuilder> _statements;
}

public readonly struct StatementBuilder {
    public ExpressionFragment Expression { get; }
    public StatementBuilder(ExpressionFragment expression) { }

    public static implicit operator StatementBuilder(ExpressionFragment expression) {
        return new StatementBuilder(expression);
    }

    public override string ToString() {
        return Expression.ToString();
    }
}