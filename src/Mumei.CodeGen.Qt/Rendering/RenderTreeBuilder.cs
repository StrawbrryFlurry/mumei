using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

internal abstract class GenericRenderTreeBuilder : IRenderTreeBuilder {
    private FeatureCollection? _features;

#if DEBUG
    private readonly DebugRenderGraph _debugRenderGraph = new();
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartNode<TRenderNode>(TRenderNode node) where TRenderNode : IRenderNode {
#if DEBUG
        _debugRenderGraph.StartNode(node);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndNode() {
#if DEBUG
        _debugRenderGraph.EndNode();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Text(ReadOnlySpan<char> s) {
        TextCore(s);
    }
    protected abstract void TextCore(ReadOnlySpan<char> s);

    public void Value<T>(in T value) { }
    protected abstract void ValueCore<T>(in T value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Interpolate(IRenderTreeBuilder.InterpolatedStringHandler s) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Block() {
        BlockCore();
    }
    protected abstract void BlockCore();

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
    public void Node<TRenderNode>(TRenderNode node) where TRenderNode : IRenderNode {
        StartNode(node);

#if !DEBUG
        NodeCore(node);
#else
        try {
            NodeCore(node);
        } catch (Exception e) {
            throw new InvalidOperationException($"Error while rendering node of type {node.GetType().FullName} at:\n{DebugView()}", e);
        }
#endif

        EndNode();
    }

    public void Node(RenderNode node) {
        node(this);
    }

    public void Node<TState>(in RenderNode<TState> node) {
        node.Render(this);
    }

    protected abstract void NodeCore<TRenderNode>(TRenderNode renderable) where TRenderNode : IRenderNode;

    public void RequireFeature(IRenderer.IFeature feature) {
        _features ??= new FeatureCollection();
        _features.Require(feature);
    }

    public string DebugView() {
#if DEBUG
        return _debugRenderGraph.DebugView();
#else
        return ToString();
#endif
    }
}

public interface IRenderTreeBuilder {
    public void StartNode<TRenderNode>(TRenderNode node) where TRenderNode : IRenderNode;
    public void EndNode();

    public void Text(ReadOnlySpan<char> s);
    public void Value<T>(in T value);

    public void Interpolate([InterpolatedStringHandlerArgument("")] InterpolatedStringHandler s);

    public void Block();
    public void StartBlock();
    public void EndBlock();

    public void Bind<TBindable>(TBindable bindable) where TBindable : IQtTemplateBindable;

    public void SyntaxNode<TNode>(TNode node) where TNode : SyntaxNode;

    public void Node<TRenderNode>(TRenderNode node) where TRenderNode : IRenderNode;
    public void Node(RenderNode render);
    public void Node<TState>(in RenderNode<TState> render);

    public void RequireFeature(IRenderer.IFeature feature);

    [InterpolatedStringHandler]
    public readonly ref struct InterpolatedStringHandler(int literalLength, int formattedCount, IRenderTreeBuilder tree) {
        public void AppendLiteral(string s) {
            tree.Text(s);
        }

        public void AppendFormatted<TRenderNode>(in TRenderNode node) where TRenderNode : IRenderNode {
            tree.Node(node);
        }

        public void AppendFormatted(ReadOnlySpan<char> s) {
            tree.Text(s);
        }
    }
}