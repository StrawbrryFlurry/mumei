using System.Reflection;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class QtSyntheticAttribute : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
}

internal sealed class RuntimeSyntheticAttribute(CustomAttributeData attributeData) : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ICompilationUnitContext compilationUnit) {
        throw new NotImplementedException();
    }
}