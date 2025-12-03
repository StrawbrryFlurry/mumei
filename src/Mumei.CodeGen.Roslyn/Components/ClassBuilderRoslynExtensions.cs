using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.Components;

public static class ClassBuilderRoslynExtensions {
    extension<TClass>(ISyntheticClassBuilder<TClass> builder) {
        public ISyntheticClassBuilder<TClass> Bind<T>(ITypeSymbol typeSymbol) {
            return builder;
        }

        public ISyntheticField<CompileTimeUnknown> DeclareField(ITypeSymbol typeSymbol, SyntheticIdentifier name) {
            return builder.DeclareField<CompileTimeUnknown>(typeSymbol.ToDisplayString());
        }

        public ISyntheticField<CompileTimeUnknown> DeclareProperty(ITypeSymbol typeSymbol, SyntheticIdentifier name) {
            return builder.DeclareField<CompileTimeUnknown>(typeSymbol.ToDisplayString());
        }
    }
}