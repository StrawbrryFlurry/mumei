namespace Mumei.CodeGen.Components;

public interface ISyntheticMethod<TSignature> : ISyntheticMethod where TSignature : Delegate {
    public TSignature Bind(object target);
    public ISyntheticDeclaration ContainingType { get; }
}

public interface ISyntheticMethod {
    public SyntheticIdentifier Name { get; }

    public TSignature BindAs<TSignature>(object target) where TSignature : Delegate;
}