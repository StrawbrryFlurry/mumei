using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

public interface ISyntheticParameter {
    public string Name { get; }
    public ISyntheticType Type { get; }
    public ParameterAttributes ParameterAttributes { get; }
}