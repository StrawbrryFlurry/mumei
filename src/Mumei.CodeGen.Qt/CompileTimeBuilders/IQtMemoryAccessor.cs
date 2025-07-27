namespace Mumei.CodeGen.Qt;

/// <summary>
/// This would ideally expose a span, but since we can't use allows ref struct
/// most consumers will not be able to use it as they would itself need to be a ref struct.
/// </summary>
/// <typeparam name="TElement"></typeparam>
public interface IQtMemoryAccessor<TElement> {
    public Memory<TElement> Memory { get; }
}

internal static class MemoryRepresentableExtensions { }