namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct BlockBuilder {
    private readonly List<StatementBuilder> _statements;
}

public readonly struct StatementBuilder : IRenderFragment {
    private readonly RenderFragment? _renderExpression;
    private readonly ExpressionFragment _expression;

    public StatementBuilder(RenderFragment renderExpression) {
        _renderExpression = renderExpression;
    }

    public StatementBuilder(ExpressionFragment expression) {
        _expression = expression;
    }

    public override string ToString() {
        return DebugRenderer.Render(this);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        if (_renderExpression is not null) {
            renderTree.Node(_renderExpression);
            renderTree.Text(";");
            return;
        }

        renderTree.Node(_expression);
        renderTree.Text(";");
    }

    public static StatementBuilder ForRenderExpression(RenderFragment expression) {
        return new StatementBuilder(expression);
    }
}