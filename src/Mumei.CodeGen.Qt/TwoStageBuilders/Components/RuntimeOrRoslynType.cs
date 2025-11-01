namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public abstract class RuntimeOrRoslynType {
    public static implicit operator RuntimeOrRoslynType(Type runtimeType) {
        return null!;
    }
}