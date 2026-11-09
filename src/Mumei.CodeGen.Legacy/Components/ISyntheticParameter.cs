using Mumei.CodeGen.Legacy.Rendering.CSharp;

namespace Mumei.CodeGen.Legacy.Components;

public interface ISyntheticParameter {
    public string Name { get; }
    public ISyntheticType Type { get; }
    public ParameterAttributes ParameterAttributes { get; }
}