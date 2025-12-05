using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.Components;

internal sealed class GlobalSyntheticNamespace(
    ICodeGenerationContext.IΦInternalCompilerApi compilerApi
) : ISyntheticNamespaceBuilder, ISyntheticConstructable<NamespaceOrGlobalScopeFragment> {
    public string FullyQualifiedName { get; } = "";

    public SyntheticIdentifier Name { get; } = SyntheticIdentifier.Constant("<>GlobalNamespace");
    public ISyntheticDeclaration? Parent { get; } = null;

    public ImmutableArray<ISyntheticDeclaration> Members => _namespaceBuilder.Members;
    public bool IsGlobalNamespace { get; } = true;

    private readonly ISyntheticNamespaceBuilder _namespaceBuilder = new QtSyntheticNamespace(
        "<global namespace>",
        null!,
        compilerApi
    );

    public NamespaceOrGlobalScopeFragment Construct(ICompilationUnitContext compilationUnit) {
        var constructedMembers = new ArrayBuilder<NamespaceOrGlobalScopeFragment>();

        foreach (var member in Members) {
            var fragment = compilationUnit.Synthesize<NamespaceOrGlobalScopeFragment>(member);
            constructedMembers.Add(fragment);
        }

        return new NamespaceOrGlobalScopeFragment(
            null,
            null,
            [],
            constructedMembers.ToImmutableArrayAndFree(),
            TriviaFragment.Empty,
            TriviaFragment.Empty
        );
    }

    public ISyntheticNamespace WithMember(ISyntheticDeclaration member) {
        _namespaceBuilder.WithMember(member);
        return this;
    }

    public ISyntheticClassBuilder<TClass> DeclareClass<TClass>(SyntheticIdentifier name) {
        return _namespaceBuilder.DeclareClass<TClass>(name).WithDeclaration(this);
    }

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(SyntheticIdentifier name) {
        return _namespaceBuilder.DeclareClass(name).WithDeclaration(this);
    }

    public ISyntheticNamespaceBuilder DeclareNamespace(string name) {
        var ns = new QtSyntheticNamespace(name, this, compilerApi);
        _namespaceBuilder.WithMember(ns);
        return ns;
    }
}