using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Roslyn.Components;

internal sealed class RoslynSyntheticAttribute(AttributeData attributeData) : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
}