using System.Reflection;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public abstract class SyntheticInterceptorMethodDefinition<TResult> {
    public virtual void BindDynamicComponents() { }

    public abstract ISyntheticCodeBlock GenerateMethodBody();

    public TResult Invoke() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public object[] InvocationArguments { get; } = null!;
    public MethodInfo Method { get; } = null!;
}