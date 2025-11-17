using System.Reflection;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public abstract class SyntheticInterceptorMethodDefinition {
    public virtual void BindDynamicComponents() { }

    public T Invoke<T>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; } = null!;
    public MethodInfo Method { get; } = null!;
}