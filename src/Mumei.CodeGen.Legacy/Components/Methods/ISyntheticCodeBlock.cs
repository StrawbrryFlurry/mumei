using Mumei.CodeGen.Legacy.Rendering;
using Mumei.CodeGen.Legacy.Rendering.CSharp;

namespace Mumei.CodeGen.Legacy.Components;

public interface ISyntheticCodeBlock { }

internal sealed class QtSyntheticRenderCodeBlock(RenderFragment renderFragment)
    : ISyntheticCodeBlock, ISyntheticConstructable<CodeBlockFragment> {
    public CodeBlockFragment Construct(ICompilationUnitContext compilationUnit) {
        return CodeBlockFragment.Create(renderFragment);
    }
}

internal sealed class QtSyntheticRenderCodeBlock<TState>(RenderFragment<TState> renderFragment)
    : ISyntheticCodeBlock, ISyntheticConstructable<CodeBlockFragment> {
    public CodeBlockFragment Construct(ICompilationUnitContext compilationUnit) {
        return CodeBlockFragment.Create(renderFragment, (renderTree, state) => state.Render(renderTree));
    }
}