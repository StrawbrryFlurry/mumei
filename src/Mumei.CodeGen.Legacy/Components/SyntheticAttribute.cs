using System.Reflection;
using Mumei.CodeGen.Legacy.Rendering.CSharp;

namespace Mumei.CodeGen.Legacy.Components;

internal sealed class QtSyntheticAttribute : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
}

internal sealed class RuntimeSyntheticAttribute(CustomAttributeData attributeData)
    : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
}