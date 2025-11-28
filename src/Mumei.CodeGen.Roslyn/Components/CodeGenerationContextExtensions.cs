using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Roslyn.Components;

public static class CodeGenerationContextExtensions {
    extension(ICodeGenerationContext ctx) {
        public Compilation Compilation => ctx.GetContextProvider<CompilationCodeGenerationContextProvider>().Compilation;

        public ITypeSymbol TypeFromCompilation<T>() {
            return ctx.GetContextProvider<CompilationCodeGenerationContextProvider>().Compilation.GetTypeByMetadataName(typeof(T).FullName);
        }

        public ISyntheticType Type(ITypeSymbol typeSymbol) {
            return new RoslynSyntheticType(typeSymbol);
        }

        public ISyntheticType Type(TypeSyntax typeSyntax) {
            var semanticModel = ctx.GetContextProvider<CompilationCodeGenerationContextProvider>().Compilation.GetSemanticModel(typeSyntax.SyntaxTree);
            var typeSymbol = ModelExtensions.GetTypeInfo(semanticModel, typeSyntax).Type ?? throw new InvalidOperationException("Could not resolve type symbol from syntax.");
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

        public ISyntheticParameter Parameter(string name, ITypeSymbol type, ParameterAttributes attributes = ParameterAttributes.None) {
            var parameterType = ctx.Type(type);
            return new SyntheticParameter(name, parameterType, attributes: attributes);
        }

        public ISyntheticTypeParameterList TypeParameterListFrom(IMethodSymbol methodSymbol) {
            var typeParameters = new ISyntheticTypeParameter[methodSymbol.TypeParameters.Length];
            for (var i = 0; i < methodSymbol.TypeParameters.Length; i++) {
                var typeParameter = methodSymbol.TypeParameters[i];
                typeParameters[i] = new RoslynSyntheticTypeParameter(typeParameter);
            }

            return new QtSyntheticTypeParameterList(typeParameters);
        }

        public ISyntheticAttribute InterceptLocationAttribute(InterceptableLocation location) {
            return new SyntheticInterceptLocationAttribute(location);
        }

        public ISyntheticAttribute InterceptLocationAttribute(InvocationExpressionSyntax invocationToIntercept) {
            var semanticModel = ctx.Compilation.GetSemanticModel(invocationToIntercept.SyntaxTree);
            var location = semanticModel.GetInterceptableLocation(invocationToIntercept)
                           ?? throw new InvalidOperationException("Could not resolve interceptable location from invocation syntax.");
            return new SyntheticInterceptLocationAttribute(location);
        }
    }
}