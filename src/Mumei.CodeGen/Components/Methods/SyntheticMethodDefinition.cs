namespace Mumei.CodeGen.Components;

public abstract class SyntheticMethodDefinition : SyntheticDeclarationDefinition {
    public virtual void BindDynamicComponents() { }

    public virtual ISyntheticMethodBuilder<Delegate> InternalBindCompilerMethod(
        ISimpleSyntheticClassBuilder builder,
        Delegate targetMethod
    ) {
        throw new InvalidOperationException("Method body generation not implemented.");
    }
}