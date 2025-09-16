using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

public interface IRenderer {
    public void Text(ReadOnlySpan<char> s);

    public void Block();

    public void StartBlock();
    public void EndBlock();

    public void Interpolate([InterpolatedStringHandlerArgument(argument: "")] InterpolatedStringHandler s);

    [Obsolete("Replace with the renderer API")]
    public void Bind<TBindable>(TBindable bindable) where TBindable : IQtTemplateBindable;

    public void SyntaxNode<TNode>(TNode node) where TNode : SyntaxNode;

    public void Node<TRenderNode>(TRenderNode node) where TRenderNode : IRenderNode;

    public void RequireFeature(IFeature feature);

    public interface IFeature { }

    [InterpolatedStringHandler]
    public readonly ref struct InterpolatedStringHandler(int literalLength, int formattedCount, IRenderer renderer) {
        public void AppendLiteral(string s) {
            renderer.Text(s);
        }

        public void AppendFormatted<TRenderNode>(in TRenderNode node) where TRenderNode : IRenderNode {
            renderer.Node(node);
        }

        public void AppendFormatted(ReadOnlySpan<char> s) {
            renderer.Text(s);
        }
    }

    public delegate void Render<in TState>(IRenderer renderer, TState state);
}