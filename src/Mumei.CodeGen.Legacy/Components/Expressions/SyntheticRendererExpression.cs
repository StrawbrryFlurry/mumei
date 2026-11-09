using Mumei.CodeGen.Legacy.Rendering;
using Mumei.CodeGen.Legacy.Rendering.CSharp;

namespace Mumei.CodeGen.Legacy.Components;

internal sealed class SyntheticRendererExpression(
    RenderFragment<object> renderFragment
) : ISyntheticExpression, ISyntheticConstructable<ExpressionFragment> {
    public ExpressionFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
}