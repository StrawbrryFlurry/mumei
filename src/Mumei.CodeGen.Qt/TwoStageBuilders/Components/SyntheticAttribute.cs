using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticAttribute : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ISyntheticCompilation compilation) {
        throw new NotImplementedException();
    }
}

internal sealed class RuntimeSyntheticAttribute(CustomAttributeData attributeData) : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ISyntheticCompilation compilation) {
        throw new NotImplementedException();
    }
}

internal sealed class RoslynSyntheticAttribute(AttributeData attributeData) : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ISyntheticCompilation compilation) {
        throw new NotImplementedException();
    }
}