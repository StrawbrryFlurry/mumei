using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class QtSyntheticAttributeList(ReadOnlySpan<ISyntheticAttribute> attributes) : ISyntheticAttributeList, ISyntheticConstructable<ImmutableArray<AttributeFragment>> {
    private List<ISyntheticAttribute>? _attributes = attributes.Length > 0 ? [..attributes] : null;

    public static QtSyntheticAttributeList FromAttributeData(ImmutableArray<AttributeData> attributeData) {
        var attributeList = new QtSyntheticAttributeList([]);
        foreach (var attribute in attributeData) {
            attributeList.AddAttribute(new RoslynSyntheticAttribute(attribute));
        }

        return attributeList;
    }

    public void AddAttribute(ISyntheticAttribute attribute) {
        _attributes ??= [];
        _attributes.Add(attribute);
    }

    public ImmutableArray<AttributeFragment> Construct(ISyntheticCompilation compilation) {
        var attributes = _attributes ?? [];
        var result = ImmutableArray.CreateBuilder<AttributeFragment>(attributes.Count);
        foreach (var attribute in attributes) {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (attribute is not ISyntheticConstructable<AttributeFragment> constructable) {
                throw new NotSupportedException();
            }

            result.Add(constructable.Construct(compilation));
        }

        return result.ToImmutable();
    }
}