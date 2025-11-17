namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public abstract class SyntheticFieldRef<T> {
    public static implicit operator SyntheticFieldRef<T>(T value) {
        throw new NotSupportedException();
    }

    public static implicit operator T(SyntheticFieldRef<T> value) {
        throw new NotSupportedException();
    }
}