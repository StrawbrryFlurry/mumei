namespace Mumei.CodeGen.Components;

public sealed class SyntheticMethodRef<TSignature> where TSignature : Delegate {
    public TSignature Invoke { get; }

    public static implicit operator SyntheticMethodRef<TSignature>(TSignature value) {
        throw new NotSupportedException();
    }

    public static implicit operator TSignature(SyntheticMethodRef<TSignature> value) {
        throw new NotSupportedException();
    }
}