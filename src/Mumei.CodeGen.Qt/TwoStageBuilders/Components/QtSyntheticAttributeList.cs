using System.Collections.Immutable;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticAttributeList : ISyntheticConstructable<ImmutableArray<AttributeFragment>> {
    private readonly List<ISyntheticAttribute> _attributes = new();

    public void AddAttribute(ISyntheticAttribute attribute) {
        _attributes.Add(attribute);
    }

    public ImmutableArray<AttributeFragment> Construct() {
        var result = ImmutableArray.CreateBuilder<AttributeFragment>(_attributes.Count);
        foreach (var attribute in _attributes) {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (attribute is not ISyntheticConstructable<AttributeFragment> constructable) {
                throw new NotSupportedException();
            }

            result.Add(constructable.Construct());
        }

        return result.ToImmutable();
    }
}