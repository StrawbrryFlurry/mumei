using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.Components;

public static class ClassBuilderRoslynExtensions {
    extension(ISimpleClassBuilder builder) {
        public void Bind(Type toBind, ITypeSymbol typeSymbol, [CallerArgumentExpression(nameof(toBind))] string bindingTargetExpression = "") {
            builder.ΦCompilerApi.Context.Type(typeSymbol);
            builder.Bind(toBind, typeSymbol, bindingTargetExpression);
        }
    }

    extension<TClass>(ISyntheticClassBuilder<TClass> builder) {
        public ISyntheticField<CompileTimeUnknown> DeclareField(ITypeSymbol typeSymbol, SyntheticIdentifier name) {
            return null!;
        }

        public ISyntheticProperty<CompileTimeUnknown> DeclareProperty(ITypeSymbol typeSymbol, SyntheticIdentifier name) {
            return null!;
        }
    }
}