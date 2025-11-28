namespace Mumei.CodeGen.Components;

public interface IComponentSynthesizer {
    public T Synthesize<T>(object? constructable, T? defaultValue = default);
    public T? SynthesizeOptional<T>(object? constructable);
}