using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticNamespace {
    public ISyntheticNamespace WithMember(ISyntheticMember member);
}

internal sealed class QtSyntheticNamespace(string name) : ISyntheticNamespace, ISyntheticConstructable<NamespaceFragment> {
    public string Name { get; } = name;

    private List<ISyntheticMember>? _members;

    public ISyntheticNamespace WithMember(ISyntheticMember member) {
        _members ??= new List<ISyntheticMember>();
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