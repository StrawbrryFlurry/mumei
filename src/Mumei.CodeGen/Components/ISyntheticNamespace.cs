using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Common.Internal;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

public interface ISyntheticNamespace {
    public string FullyQualifiedName { get; }

    public ImmutableArray<ISyntheticDeclaration> Members { get; }

    public bool IsGlobalNamespace { get; }

    public ISyntheticNamespace WithMember(ISyntheticDeclaration member);
}

public interface ISyntheticNamespaceBuilder : ISyntheticNamespace, ISyntheticDeclaration {
    public ISyntheticClassBuilder<TClass> DeclareClass<TClass>(SyntheticIdentifier name);
    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(SyntheticIdentifier name);
    public ISyntheticNamespaceBuilder DeclareNamespace(string name);
}

internal sealed class QtSyntheticNamespace(
    string name,
    ISyntheticDeclaration parent,
    ICodeGenerationContext.IΦInternalCompilerApi compilerApi
) : ISyntheticNamespaceBuilder, ISyntheticConstructable<NamespaceOrGlobalScopeFragment> {
    public SyntheticIdentifier Name { get; } = name;
    public ISyntheticDeclaration Parent { get; } = parent;

    public string FullyQualifiedName { get; } = name;
    public SyntheticIdentifier Identifier { get; } = SyntheticIdentifier.Constant(name);

    public ImmutableArray<ISyntheticDeclaration> Members => _members?.ToImmutableArray() ?? [];
    public bool IsGlobalNamespace { get; } = false;
    private List<ISyntheticDeclaration>? _members;

    public ISyntheticNamespace WithMember(ISyntheticDeclaration member) {
        _members ??= [];
        _members.Add(member);
        return this;
    }

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(SyntheticIdentifier name) {
        return DeclareClass<CompileTimeUnknown>(name);
    }

    public ISyntheticNamespaceBuilder DeclareNamespace(string name) {
        var namespaceBuilder = new QtSyntheticNamespace(name, this, compilerApi);
        WithMember(namespaceBuilder);
        return namespaceBuilder;
    }

    public ISyntheticClassBuilder<TClass> DeclareClass<TClass>(SyntheticIdentifier name) {
        var cls = compilerApi.DeclareClassBuilder<TClass>(name, this);
        WithMember(cls);
        return cls;
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