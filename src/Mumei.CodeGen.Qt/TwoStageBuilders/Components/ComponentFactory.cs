namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public sealed class ComponentFactory {
    public static ISyntheticClassBuilder Class() {
        return new SyntheticClassBuilder();
    }

    public static ISyntheticClassBuilder<TDef> Class<TDef>(Action<TDef> bindInputs) {
        return null!;
    }
}