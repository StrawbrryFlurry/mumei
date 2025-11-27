using System.Collections.Immutable;
using Mumei.CodeGen.Components.Types;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

public interface ISyntheticNamespace {
    public string FullyQualifiedName { get; }

    public ImmutableArray<ISyntheticMember> Members { get; }

    public ISyntheticNamespace WithMember(ISyntheticMember member);
}

internal sealed class QtSyntheticNamespace(string name) : ISyntheticNamespace, ISyntheticConstructable<NamespaceFragment> {
    public string Name { get; } = name;

    public string FullyQualifiedName { get; } = name;

    public ImmutableArray<ISyntheticMember> Members => _members?.ToImmutableArray() ?? [];
    private List<ISyntheticMember>? _members;

    public ISyntheticNamespace WithMember(ISyntheticMember member) {
        _members ??= [];
        _members.Add(member);
        return this;
    }

    public NamespaceFragment Construct(ISyntheticCompilation compilation) {
        var classDeclarations = new ArrayBuilder<ClassDeclarationFragment>();

        if (_members is null) {
            return NamespaceFragment.Create(Name, []);
        }

        foreach (var member in _members) {
            if (member is ISyntheticClass classDeclaration) {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (member is not ISyntheticConstructable<ClassDeclarationFragment> syntheticClass) {
                    throw new InvalidOperationException(
                        $"Synthetic class member {classDeclaration.Name} of namespace {Name} must be constructable to a {nameof(ClassDeclarationFragment)} in order to be used inside a namespace constructable."
                    );
                }

                classDeclarations.Add(syntheticClass.Construct(compilation));
                continue;
            }
        }

        return NamespaceFragment.Create(Name, classDeclarations.ToImmutableArrayAndFree());
    }
}