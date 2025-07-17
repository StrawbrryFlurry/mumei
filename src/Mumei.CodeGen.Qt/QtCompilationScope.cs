using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Qt;

public sealed class QtCompilationScope {
    public required Compilation Compilation { get; init; }
    private static AsyncLocal<QtCompilationScope> _activeScope { get; } = new();

    public static QtCompilationScope ActiveScope => _activeScope.Value ?? throw new InvalidOperationException();

    public static void SetActiveScope(Compilation scope) {
        _activeScope.Value = new QtCompilationScope { Compilation = scope };
    }
}