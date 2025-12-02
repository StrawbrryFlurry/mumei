namespace Mumei.CodeGen.Components;

internal sealed class QtSyntheticMethod : ISyntheticMethod {
    public QtSyntheticMethod(
        ISyntheticAttribute[] attributes,
        AccessModifierList modifiers,
        ISyntheticType returnType,
        SyntheticIdentifier name,
        ISyntheticTypeParameter[] typeParameters,
        ISyntheticParameter[] parameters,
        ISyntheticCodeBlock body
    ) {
        Name = name;
    }

    public SyntheticIdentifier Name { get; }

    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate {
        throw new NotImplementedException();
    }
}