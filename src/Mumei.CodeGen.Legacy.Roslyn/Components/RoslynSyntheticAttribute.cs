using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Legacy.Components;
using Mumei.CodeGen.Legacy.Rendering.CSharp;

namespace Mumei.CodeGen.Legacy.Roslyn.Components;

internal sealed class RoslynSyntheticAttribute(AttributeData attributeData)
    : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
}