using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class SyntheticField<TField>(
    ISyntheticAttributeList? attributes,
    AccessModifierList modifiers,
    ISyntheticType type,
    SyntheticIdentifier name
) : ISyntheticField<TField>, ISyntheticConstructable<FieldDeclarationFragment> {
    public ISyntheticAttributeList? Attributes { get; } = attributes;
    public AccessModifierList Modifiers { get; } = modifiers;
    public ISyntheticType Type { get; } = type;
    public SyntheticIdentifier Name { get; } = name;

    public FieldDeclarationFragment Construct(ICompilationUnitContext compilationUnit) {
        return new FieldDeclarationFragment(
            compilationUnit.SynthesizeOptional<AttributeListFragment>(Attributes),
            Modifiers,
            compilationUnit.Synthesize<TypeInfoFragment>(Type),
            Name.Resolve(compilationUnit)
        );
    }
}