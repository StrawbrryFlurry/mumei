using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Qt;

public readonly struct RenderNode<TState>(TState state, Action<IRenderTreeBuilder, TState> renderTree) : IRenderNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Render(IRenderTreeBuilder outerRenderTree) {
        renderTree(outerRenderTree, state);
    }
}

public delegate void RenderNode(IRenderTreeBuilder renderTree);