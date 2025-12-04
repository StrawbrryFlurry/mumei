using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Components;

public interface ISimpleClassBuilder {
    public IΦInternalClassBuilderCompilerApi ΦCompilerApi { get; }

    public void Bind(Type t, ISyntheticType actualType, [CallerArgumentExpression(nameof(t))] string bindingTargetExpression = "");
}