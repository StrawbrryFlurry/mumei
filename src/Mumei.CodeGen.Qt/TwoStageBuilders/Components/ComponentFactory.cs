using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public sealed class ComponentFactory {
    public static ISyntheticClassBuilder<CompileTimeUnknown> Class() {
        return new SyntheticClassBuilder<CompileTimeUnknown>();
    }

    public static ISyntheticClassBuilder<TSyntheticClass> Class<TSyntheticClass>() where TSyntheticClass : ISyntheticClass {
        return null!;
    }

    public static ISyntheticClassBuilder<TSyntheticClass> Class<TSyntheticClass>(Action<TSyntheticClass> bindInputs) where TSyntheticClass : SyntheticClassDefinition<TSyntheticClass>, new() {
        return null!;
    }
}