using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;

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

    extension(AttributeFragment) {
        public static AttributeFragment Intercept(
            InterceptableLocation location
        ) {
            var interceptableLocationType = new TypeInfoFragment("global::System.Runtime.CompilerServices.InterceptsLocationAttribute");
            AttributeArgumentListFragment.Builder builder = [
                new PositionalArgumentFragment(location.Version.ToString()),
                new PositionalArgumentFragment("\"" + location.Data + "\"")
            ];
            return new AttributeFragment(interceptableLocationType, builder);
        }
    }
}