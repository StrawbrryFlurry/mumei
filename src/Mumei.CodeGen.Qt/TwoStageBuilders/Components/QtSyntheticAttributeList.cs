using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticAttributeList : ISyntheticAttributeList, ISyntheticConstructable<ImmutableArray<AttributeFragment>> {
    private readonly List<ISyntheticAttribute> _attributes = new();

    public static QtSyntheticAttributeList FromAttributeData(ImmutableArray<AttributeData> attributeData) {
        var attributeList = new QtSyntheticAttributeList();
        foreach (var attribute in attributeData) {
            attributeList.AddAttribute(new RoslynSyntheticAttribute(attribute));
        }
        return attributeList;
    }

    public void AddAttribute(ISyntheticAttribute attribute) {
        _attributes.Add(attribute);
    }

    public ImmutableArray<AttributeFragment> Construct(ISyntheticCompilation compilation) {
        var result = ImmutableArray.CreateBuilder<AttributeFragment>(_attributes.Count);
        foreach (var attribute in _attributes) {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (attribute is not ISyntheticConstructable<AttributeFragment> constructable) {
                throw new NotSupportedException();
            }

            result.Add(constructable.Construct(compilation));
        }

        return result.ToImmutable();
    }
}