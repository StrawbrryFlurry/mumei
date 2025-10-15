using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mumei.CodeGen.Qt;

public sealed class QtCompilationScope : IEquatable<QtCompilationScope> {
    public static QtCompilationScope Active => ActiveScope.Value ?? throw new InvalidOperationException("No active compilation scope. Ensure that you are inside a source generator context.");
    private static AsyncLocal<QtCompilationScope> ActiveScope { get; } = new();
    private readonly HashSet<ICompilationUnitFeature> _compilationUnitFeatures = [];

    public required Compilation Compilation { get; init; }

    internal readonly Dictionary<string, SourceText> GeneratedFiles = new();

    public static void SetActiveScope(Compilation scope) {
        ActiveScope.Value = new QtCompilationScope { Compilation = scope };
    }

    internal static void RequiresFeature(ICompilationUnitFeature feature) {
        Active._compilationUnitFeatures.Add(feature);
    }

    public override int GetHashCode() {
        return 0;
    }

    public override bool Equals(object? obj) {
        return obj is QtCompilationScope other && Equals(other);
    }

    public bool Equals(QtCompilationScope other) {
        return true; // Source generator caching
    }
}

public static class SourceProductionContextExtensions {
    public static void AddSourceOutput(this in SourceProductionContext context, QtCompilationScope scope) {
        foreach (var file in scope.GeneratedFiles) {
            context.AddSource(file.Key, file.Value);
        }
    }
}