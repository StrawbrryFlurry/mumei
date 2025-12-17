namespace Mumei.CodeGen.Components;

public interface ISyntheticParameterList : IReadOnlyList<ISyntheticParameter> {
    public void InsertAt(int i, ISyntheticParameter parameter);
    public ReadOnlySpan<ISyntheticParameter> AsSpan();
}