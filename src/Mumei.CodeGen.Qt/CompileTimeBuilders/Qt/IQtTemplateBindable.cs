using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public interface IQtTemplateBindable : ISyntaxRepresentable {
    // TODO: The actual implementation should probably look like this instead of using ISyntaxRepresentable
    // but this would require a big refactor without the certainty that this solves all the problems.
    // public void Bind<TSyntaxWriter>(TemplateBindingContext ctx, TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter;
}

public sealed class TemplateBindingContext : IDisposable {
    private bool _disposed;
    private TemplateBindingContext? _parent;

    [ThreadStatic] // Can be thread static because the template binding (ISyntaxRepresentable) is never async
    private static TemplateBindingContext? _activeContext;

    internal CodeGenFeatureCollection CodeGenFeatures { get; } = new();

    public static TemplateBindingContext Current {
        get {
            if (_activeContext is null) {
                throw new InvalidOperationException("No active template binding context. Ensure that you are inside a template binding operation.");
            }

            if (_activeContext._disposed) {
                throw new ObjectDisposedException(nameof(TemplateBindingContext), "The current template binding context has been disposed. Ensure that you are not using the context outside of a template binding operation.");
            }

            return _activeContext;
        }
    }

    private TemplateBindingContext(TemplateBindingContext? parent) {
        _parent = parent;
    }

    public static TemplateBindingContext StartBinding() {
        return _activeContext = new TemplateBindingContext(_activeContext);
    }

    public void Dispose() {
        _disposed = true;
        _activeContext = _parent;
    }
}