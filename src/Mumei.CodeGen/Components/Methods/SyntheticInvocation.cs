namespace Mumei.CodeGen.Components;

public sealed class SyntheticInvocation<TResult> {
    public static implicit operator SyntheticInvocation<TResult>(TResult value) {
        throw new NotSupportedException();
    }
}