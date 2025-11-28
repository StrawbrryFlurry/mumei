using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.Components;

public static class CodeGenerationContextExtensions {
    extension(ICodeGenerationContext ctx) {
        public ITypeSymbol TypeFromCompilation<T>() {
            return ctx.GetSynthesisProvider<CompilationSynthesisProvider>().Compilation.GetTypeByMetadataName(typeof(T).FullName);
        }

        public ISyntheticType Type(ITypeSymbol typeSymbol) {
            return new RoslynSyntheticType(typeSymbol);
        }

        public ISyntheticType Type(TypeSyntax typeSyntax) {
            var semanticModel = ctx.GetSynthesisProvider<CompilationSynthesisProvider>().Compilation.GetSemanticModel(typeSyntax.SyntaxTree);
            var typeSymbol = semanticModel.GetTypeInfo(typeSyntax).Type ?? throw new InvalidOperationException("Could not resolve type symbol from syntax.");
            return ctx.Type(typeSymbol);
        }

        public ISyntheticType Type(Type type) {
            return new RuntimeSyntheticType(type);
        }

        public ISyntheticParameterList ParameterListFrom(IMethodSymbol methodSymbol) {
            var parameters = new ISyntheticParameter[methodSymbol.Parameters.Length];
            for (var i = 0; i < methodSymbol.Parameters.Length; i++) {
                var parameter = methodSymbol.Parameters[i];
                parameters[i] = new RoslynSyntheticParameter(parameter);
            }

            return new QtSyntheticParameterList(parameters);
        }

        public ISyntheticTypeParameterList TypeParameterListFrom(IMethodSymbol methodSymbol) {
            var typeParameters = new ISyntheticTypeParameter[methodSymbol.TypeParameters.Length];
            for (var i = 0; i < methodSymbol.TypeParameters.Length; i++) {
                var typeParameter = methodSymbol.TypeParameters[i];
                typeParameters[i] = new RoslynSyntheticTypeParameter(typeParameter);
            }

            return new QtSyntheticTypeParameterList(typeParameters);
        }
    }
}