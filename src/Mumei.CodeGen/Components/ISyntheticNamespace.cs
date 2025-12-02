using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

public interface ISyntheticNamespace : ISyntheticDeclaration {
    public string FullyQualifiedName { get; }

    public ImmutableArray<ISyntheticMember> Members { get; }

    public ISyntheticNamespace WithMember(ISyntheticMember member);
}

internal sealed class QtSyntheticNamespace(string name) : ISyntheticNamespace, ISyntheticConstructable<NamespaceOrGlobalScopeFragment> {
    public SyntheticIdentifier Name { get; } = name;

    public string FullyQualifiedName { get; } = name;
    public SyntheticIdentifier Identifier { get; } = SyntheticIdentifier.Constant(name);

    public ImmutableArray<ISyntheticMember> Members => _members?.ToImmutableArray() ?? [];
    private List<ISyntheticMember>? _members;

    public ISyntheticNamespace WithMember(ISyntheticMember member) {
        _members ??= [];
        _members.Add(member);
        return this;
    }

    public NamespaceOrGlobalScopeFragment Construct(ICompilationUnitContext compilationUnit) {
        var classDeclarations = new ArrayBuilder<ClassDeclarationFragment>();

        if (_members is null) {
            return NamespaceOrGlobalScopeFragment.Create(Name.Resolve(compilationUnit), []);
        }

        foreach (var member in _members) {
            if (member is ISyntheticClass classDeclaration) {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (member is not ISyntheticConstructable<ClassDeclarationFragment> syntheticClass) {
                    throw new InvalidOperationException(
                        $"Synthetic class member {classDeclaration.Name} of namespace {Name} must be constructable to a {nameof(ClassDeclarationFragment)} in order to be used inside a namespace constructable."
                    );
                }

                classDeclarations.Add(syntheticClass.Construct(compilationUnit));
                continue;
            }
        }

        return NamespaceOrGlobalScopeFragment.Create(Name.Resolve(compilationUnit), classDeclarations.ToImmutableArrayAndFree());
    }
}