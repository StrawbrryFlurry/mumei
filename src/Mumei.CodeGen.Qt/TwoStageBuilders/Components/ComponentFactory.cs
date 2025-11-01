namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public sealed class ComponentFactory {
    public static IClassComponentBuilder Class() {
        return new ClassComponentBuilder();
    }
}