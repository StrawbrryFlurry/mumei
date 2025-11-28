using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class SyntheticAttributeList(ReadOnlySpan<ISyntheticAttribute> attributes) : ISyntheticAttributeList, ISyntheticConstructable<ImmutableArray<AttributeFragment>> {
    private List<ISyntheticAttribute>? _attributes = attributes.Length > 0 ? [..attributes] : null;

    public void AddAttribute(ISyntheticAttribute attribute) {
        _attributes ??= [];
        _attributes.Add(attribute);
    }

    public ImmutableArray<AttributeFragment> Construct(ICompilationUnitContext compilationUnit) {
        var attributes = _attributes ?? [];
        var result = ImmutableArray.CreateBuilder<AttributeFragment>(attributes.Count);
        foreach (var attribute in attributes) {
            result.Add(compilationUnit.Synthesize<AttributeFragment>(attribute));
        }

        return result.ToImmutable();
    }
}