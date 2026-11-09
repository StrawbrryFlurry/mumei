using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Legacy.Components;

namespace Mumei.CodeGen.Legacy.Roslyn.Components;

public static class ClassBuilderRoslynExtensions {
    extension<TClass>(ISyntheticClassBuilder<TClass> builder) {
        public ISyntheticField<CompileTimeUnknown> DeclareField(ITypeSymbol typeSymbol, SyntheticIdentifier name) {
            return null!;
        }

        public ISyntheticPropertyBuilder<CompileTimeUnknown> DeclareProperty(ITypeSymbol typeSymbol,
            SyntheticIdentifier name) {
            return null!;
        }
    }
}