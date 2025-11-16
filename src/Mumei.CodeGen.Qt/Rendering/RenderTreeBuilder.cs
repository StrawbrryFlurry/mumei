using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

internal abstract class GenericRenderTreeBuilder<TResult>(FeatureCollection? parentFeatureCollection) : IRenderTreeBuilder {
    private FeatureCollection? _features = parentFeatureCollection;
    protected FeatureCollection? Features => _features;

#if DEBUG
    protected readonly DebugRenderGraph DebugRenderGraph = new();
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartNode<TRenderNode>(TRenderNode node) where TRenderNode : IRenderFragment {
#if DEBUG
        DebugRenderGraph.StartNode(node);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndNode() {
#if DEBUG
        DebugRenderGraph.EndNode();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Text(ReadOnlySpan<char> s) {
        TextCore(s);
    }
    protected abstract void TextCore(ReadOnlySpan<char> s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NewLine() {
        NewLineCore();
    }
    protected abstract void NewLineCore();

    public void Value<T>(in T value) { }
    protected abstract void ValueCore<T>(in T value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Interpolate([InterpolatedStringHandlerArgument("")] IRenderTreeBuilder.InterpolatedStringHandler s) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Block(string s) {
        BlockCore(s);
    }
    protected abstract void BlockCore(string s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InterpolateBlock(IRenderTreeBuilder.BlockInterpolatedStringHandler s) {
        BlockCore(s.GetSourceText());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartBlock() {
        StartBlockCore();
    }
    protected abstract void StartBlockCore();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndBlock() {
        EndBlockCore();
    }
    protected abstract void EndBlockCore();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Bind<TBindable>(TBindable bindable) where TBindable : IQtTemplateBindable {
        BindCore(bindable);
    }
    protected abstract void BindCore<TBindable>(TBindable bindable) where TBindable : IQtTemplateBindable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SyntaxNode<TNode>(TNode node) where TNode : SyntaxNode {
#if !DEBUG
        SyntaxNodeCore(node);
#else
        try {
            SyntaxNodeCore(node);
        } catch (Exception e) {
            throw new InvalidOperationException($"Error while rendering syntax node of type {node.GetType().FullName} at:\n{DebugView()}", e);
        }
#endif
    }

    protected abstract void SyntaxNodeCore<TNode>(TNode node) where TNode : SyntaxNode;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Node<TRenderNode>(TRenderNode node) where TRenderNode : IRenderFragment {
        StartNode(node);

#if !DEBUG
        NodeCore(node);
#else
        try {
            NodeCore(node);
        } catch (Exception e) when (e is not RendererException) {
            var nodeId = node is IDebugRenderNodeFormattable debugNode ? debugNode.DescribeDebugNode() : $"of type {node.GetType().FullName}";
            throw new RendererException($"Error while rendering node {nodeId} at:\n{DebugView()}", e);
        }
#endif

        EndNode();
    }

    public void Node(RenderFragment fragment) {
        fragment(this);
    }

    public void Node<TState>(in RenderFragment<TState> fragment) {
        fragment.Render(this);
    }

    protected abstract void NodeCore<TRenderNode>(TRenderNode renderable) where TRenderNode : IRenderFragment;

    public abstract TResult RenderRootNode<TRootNode>(TRootNode node) where TRootNode : IRenderFragment;

    public void RequireFeature(IRenderer.IFeature feature) {
        _features ??= new FeatureCollection();
        _features.Require(feature);
    }

    public string DebugView() {
#if DEBUG
        return DebugRenderGraph.DebugView();
#else
        return ToString();
#endif
    }

    public sealed class RendererException(string msg, Exception inner) : Exception(msg, inner);
}

public interface IRenderTreeBuilder {
    public void StartNode<TRenderNode>(TRenderNode node) where TRenderNode : IRenderFragment;
    public void EndNode();

    public void Text(ReadOnlySpan<char> s);
    public void NewLine();
    public void Value<T>(in T value);

    public void Interpolate([InterpolatedStringHandlerArgument("")] InterpolatedStringHandler s);

    public void Block(string s);
    public void InterpolateBlock(BlockInterpolatedStringHandler s);
    public void StartBlock();
    public void EndBlock();

    public void Bind<TBindable>(TBindable bindable) where TBindable : IQtTemplateBindable;

    public void SyntaxNode<TNode>(TNode node) where TNode : SyntaxNode;

    public void Node<TRenderNode>(TRenderNode node) where TRenderNode : IRenderFragment;
    public void Node(RenderFragment render);
    public void Node<TState>(in RenderFragment<TState> render);

    public void RequireFeature(IRenderer.IFeature feature);

    [InterpolatedStringHandler]
    public readonly ref struct InterpolatedStringHandler(int literalLength, int formattedCount, IRenderTreeBuilder tree) {
        public void AppendLiteral(string s) {
            tree.Text(s);
        }

        public void AppendFormatted<TRenderNode>(in TRenderNode node) where TRenderNode : IRenderFragment {
            tree.Node(node);
        }

        public void AppendFormatted(Type type, string format = "") {
            if (format == "typeof") {
                tree.TypeOf(type);
                return;
            }

            tree.QualifiedTypeName(type);
        }

        public void AppendFormatted(ReadOnlySpan<char> s) {
            tree.Text(s);
        }
    }

    [InterpolatedStringHandler]
    public readonly ref struct BlockInterpolatedStringHandler(int literalLength, int formattedCount) {
        // ToDo: Implement block handling via the source render tree.
        private readonly SourceFileRenderTreeBuilder _tree = new();

        public void AppendLiteral(string s) {
            _tree.Text(s);
        }

        public void AppendFormatted<TRenderNode>(in TRenderNode node) where TRenderNode : IRenderFragment {
            _tree.Node(node);
        }

        public void AppendFormatted(Type type, string format = "") {
            if (format == "typeof") {
                _tree.TypeOf(type);
                return;
            }

            _tree.QualifiedTypeName(type);
        }

        public void AppendFormatted(ReadOnlySpan<char> s) {
            _tree.Text(s);
        }

        public string GetSourceText() {
            return _tree.GetSourceText();
        }
    }
}