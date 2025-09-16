using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

[DebuggerDisplay("{DebugView(),nq}")]
public abstract class GenericRenderer : IRenderer {
#if DEBUG
    private readonly DebugRenderGraph _debugRenderGraph = new();
#endif

    private HashSet<IRenderer.IFeature>? _featureCollection;
    protected HashSet<IRenderer.IFeature>? FeatureCollection => _featureCollection;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void StartNode<TRenderNode>(TRenderNode node) where TRenderNode : IRenderNode {
#if DEBUG
        _debugRenderGraph.StartNode(node);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void EndNode() {
#if DEBUG
        _debugRenderGraph.EndNode();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Text(ReadOnlySpan<char> s) {
        TextCore(s);
    }
    protected abstract void TextCore(ReadOnlySpan<char> s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Interpolate(IRenderer.InterpolatedStringHandler s) { }

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
    protected abstract void NodeCore<TRenderNode>(TRenderNode renderable) where TRenderNode : IRenderNode;

    public void RequireFeature(IRenderer.IFeature feature) {
        _featureCollection ??= new HashSet<IRenderer.IFeature>();
        _featureCollection.Add(feature);
    }

    protected string DebugView() {
#if DEBUG
        return _debugRenderGraph.DebugView();
#else
        return ToString();
#endif
    }

}