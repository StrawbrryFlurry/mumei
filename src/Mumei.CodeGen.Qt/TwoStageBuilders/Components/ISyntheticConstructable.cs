namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal interface ISyntheticConstructable<out TTarget> {
    public TTarget Construct();
}