using System.Runtime.CompilerServices;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

public readonly struct QtExpression : IQtTemplateBindable, IRenderNode {
    private readonly string? _expression;
    private readonly IQtTemplateBindable? _dynamicExpression;
    private readonly IRenderNode? _node;

    private QtExpression(string? expression, IQtTemplateBindable? o) {
        _expression = expression;
        _dynamicExpression = o;
    }

    [OverloadResolutionPriority(1)]
    private QtExpression(string? expression, IRenderNode? node) {
        _expression = expression;
        _node = node;
    }

    public static QtExpression Null => new("null", null);

    public static QtExpression For(string expression) {
        return new QtExpression(expression, null);
    }

    public static QtExpression ForExpression(FormattableSyntaxWritable expression) {
        var outputWriter = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
        expression.CopyToAndDispose(ref outputWriter);
        return new QtExpression(outputWriter.ToString(), null);
    }

    public static QtExpression ForBindable(IQtTemplateBindable dynamicExpression) {
        return new QtExpression(null, dynamicExpression);
    }

    public static QtExpression ForNode(IRenderNode node) {
        return new QtExpression(null, node);
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

    public void Render(IRenderer renderer) {
        if (_expression is not null) {
            renderer.Text(_expression);
            return;
        }

        if (_node is null) {
            throw new ArgumentNullException(nameof(_node));
        }

        renderer.Node(_node);
    }
}