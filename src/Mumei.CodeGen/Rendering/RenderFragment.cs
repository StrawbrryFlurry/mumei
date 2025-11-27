using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Rendering;

public readonly struct RenderFragment<TState>(TState state, Action<IRenderTreeBuilder, TState> renderTree) : IRenderFragment {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Render(IRenderTreeBuilder outerRenderTree) {
        renderTree(outerRenderTree, state);
    }
}

public delegate void RenderFragment(IRenderTreeBuilder renderTree);