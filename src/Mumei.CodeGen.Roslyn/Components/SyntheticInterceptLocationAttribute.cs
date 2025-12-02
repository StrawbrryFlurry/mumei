using Microsoft.CodeAnalysis.CSharp;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Roslyn.Components;

internal sealed class SyntheticInterceptLocationAttribute(InterceptableLocation location) : ISyntheticAttribute, ISyntheticConstructable<AttributeFragment> {
    public AttributeFragment Construct(ICompilationUnitContext compilationUnit) {
        compilationUnit.AddSharedLocalCompilationUnitFeature(AddInterceptsLocationAttributeCompilationUnitFeature.Instance);
        return AttributeFragment.Intercept(location);
    }
}

internal sealed class AddInterceptsLocationAttributeCompilationUnitFeature : ICompilationUnitFeature {
    public static readonly AddInterceptsLocationAttributeCompilationUnitFeature Instance = new();
    public CompilationUnitFragment Implement(ICompilationUnitContext compilationUnit, CompilationUnitFragment currentUnit) {
        var interceptsLocationNamespace = NamespaceOrGlobalScopeFragment.Create("System.Runtime.CompilerServices", [
                ClassDeclarationFragment.Create(
                    "InterceptsLocationAttribute",
                    accessModifier: AccessModifier.File + AccessModifier.Sealed,
                    attributes: [
                        AttributeFragment.Create(
                            typeof(AttributeUsageAttribute),
                            [
                                $"{typeof(AttributeTargets)}.Method",
                                AttributePropertyArgumentFragment.Create("AllowMultiple", "true")
                            ]
                        )
                    ],
                    primaryConstructorParameters: [
                        (typeof(int), "version"),
                        (typeof(string), "data")
                    ],
                    baseTypes: [typeof(Attribute)]
                )
            ])
            .WithLeadingTrivia("#pragma warning disable\n")
            .WithTrailingTrivia("#pragma warning restore\n");

        return currentUnit.AddNamespace(interceptsLocationNamespace);

    }
}