using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.Components;

public static class SyntheticAttributeListExtensions {
    extension(ISyntheticAttributeList) {
        public static ISyntheticAttributeList From(ImmutableArray<AttributeData> attributeData) {
            var attributeList = new SyntheticAttributeList([]);
            foreach (var attribute in attributeData) {
                attributeList.AddAttribute(new RoslynSyntheticAttribute(attribute));
            }

            return attributeList;
        }
    }
}