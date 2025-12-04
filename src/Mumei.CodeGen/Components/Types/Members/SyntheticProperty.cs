using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class SyntheticProperty<TProperty>(
    ISyntheticAttributeList? attributes,
    SyntheticPropertyAccessorList accessors,
    AccessModifierList modifiers,
    ISyntheticType type,
    SyntheticIdentifier name
) : ISyntheticProperty<TProperty>, ISyntheticConstructable<PropertyDeclarationFragment> {
    public ISyntheticAttributeList? Attributes { get; } = attributes;
    public SyntheticPropertyAccessorList Accessors { get; } = accessors;
    public AccessModifierList Modifiers { get; } = modifiers;
    public ISyntheticType Type { get; } = type;
    public SyntheticIdentifier Name { get; } = name;

    public PropertyDeclarationFragment Construct(ICompilationUnitContext compilationUnit) {
        return new PropertyDeclarationFragment(
            compilationUnit.SynthesizeOptional<AttributeListFragment>(Attributes),
            Modifiers,
            compilationUnit.Synthesize<TypeInfoFragment>(Type),
            Name.Resolve(compilationUnit),
            Accessors.Getter?.Construct(compilationUnit),
            Accessors.Setter?.Construct(compilationUnit)
        );
    }
}