namespace Mumei.CodeGen.Components;

public interface ISyntheticMethodDefinition {
    public void BindDynamicComponents(MethodDefinitionBindingContext ctx);
    public ISyntheticMethodBuilder<Delegate> InternalBindCompilerMethod(
        ISimpleSyntheticClassBuilder builder,
        MethodDefinitionBindingContext bindingContext,
        Delegate targetMethod
    );
}