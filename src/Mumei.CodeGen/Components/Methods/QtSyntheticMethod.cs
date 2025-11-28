namespace Mumei.CodeGen.Components;

internal sealed class QtSyntheticMethod : ISyntheticMethod {
    public QtSyntheticMethod(
        ISyntheticAttribute[] attributes,
        AccessModifierList modifiers,
        ISyntheticType returnType,
        ISyntheticIdentifier name,
        ISyntheticTypeParameter[] typeParameters,
        ISyntheticParameter[] parameters,
        ISyntheticCodeBlock body
    ) {
        Name = name;
    }

    public ISyntheticIdentifier Name { get; }

    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate {
        throw new NotImplementedException();
    }
}