using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Qt;

public readonly struct RenderNode<TState>(TState state, Action<IRenderTreeBuilder, TState> render) : IRenderNode {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Render(IRenderTreeBuilder renderTree) {
        render(renderTree, state);
    }
}

public delegate void RenderNode(IRenderTreeBuilder renderTree);