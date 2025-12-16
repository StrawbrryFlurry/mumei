using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.Roslyn.Components;

public static class CodeGenerationContextExtensions {
    extension(ICodeGenerationContext ctx) {
        public Compilation Compilation => ctx.GetContextProvider<CompilationCodeGenerationContextProvider>().Compilation;

        public ISyntheticNamespaceBuilder NamespaceFromAssemblyName(params ReadOnlySpan<string> namespaceParts) {
            var nameBuilder = new ArrayBuilder<char>(stackalloc char[ArrayBuilder.InitSize]);
            var baseName = ctx.Compilation.Assembly.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            nameBuilder.AddRange(baseName);
            if (!namespaceParts.IsEmpty) {
                foreach (var part in namespaceParts) {
                    nameBuilder.Add('.');
                    nameBuilder.AddRange(part);
                }
            }

            var name = nameBuilder.ToStringAndFree();
            return ctx.Namespace(name);
        }

        public ISyntheticNamespaceBuilder Namespace(INamespaceSymbol namespaceSymbol) {
            return ctx.Namespace(namespaceSymbol.ToFullNameFast());
        }

        public INamedTypeSymbol TypeFromCompilation<T>() {
            return ctx.TypeFromCompilation(typeof(T));
        }

        public INamedTypeSymbol TypeFromCompilation(Type t) {
            return ctx.GetContextProvider<CompilationCodeGenerationContextProvider>().Compilation.GetTypeByMetadataName(t.FullName ?? t.Name) ?? throw new InvalidOperationException($"Could not resolve type symbol for type '{t.FullName ?? t.Name}' from compilation.");
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

            return new SyntheticParameterList(parameters);
        }

        public ISyntheticParameter Parameter(ITypeSymbol type, string name, ParameterAttributes attributes = ParameterAttributes.None) {
            var parameterType = ctx.Type(type);
            return new SyntheticParameter(name, parameterType, attributes: attributes);
        }

        public ISyntheticParameter Parameter(ITypeSymbol type, string name, out ExpressionFragment parameter) {
            var parameterType = ctx.Type(type);
            var p = new SyntheticParameter(name, parameterType, attributes: ParameterAttributes.None);
            parameter = new ExpressionFragment(name);
            return p;
        }

        public ISyntheticTypeParameter TypeParameter(string name) {
            return new QtSyntheticTypeParameter(name);
        }

        public ISyntheticTypeParameterList TypeParameterListFrom(INamedTypeSymbol typeSymbol) {
            var typeParameters = new ISyntheticTypeParameter[typeSymbol.TypeParameters.Length];
            for (var i = 0; i < typeSymbol.TypeParameters.Length; i++) {
                var typeParameter = typeSymbol.TypeParameters[i];
                typeParameters[i] = new RoslynSyntheticTypeParameter(typeParameter);
            }

            return new QtSyntheticTypeParameterList(typeParameters);
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