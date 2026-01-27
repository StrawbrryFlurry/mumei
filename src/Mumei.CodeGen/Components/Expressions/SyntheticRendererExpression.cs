using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class SyntheticRendererExpression(
    RenderFragment<object> renderFragment
) : ISyntheticExpression, ISyntheticConstructable<ExpressionFragment> {
    public ExpressionFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
}