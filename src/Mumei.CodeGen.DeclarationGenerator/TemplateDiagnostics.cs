using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mumei.CodeGen.DeclarationGenerator;

internal static class TemplateDiagnostics {
    private const string Category = "Mumei.Templates";

    public static readonly DiagnosticDescriptor NotPartial = new(
        "MUMEI101",
        "Template must be partial",
        "The template '{0}' must be declared as 'partial'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor Nested = new(
        "MUMEI102",
        "Template can't be nested",
        "The template '{0}' can't be declared inside another type",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor UnboundTypeParameter = new(
        "MUMEI103",
        "Template type parameter is not bound",
        "The type parameter '{0}' is not bound to an input. Add 'public required Type<{0}> {1} {{ get; init; }}'.",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor RuntimeValueInGenerationTimeExpression = new(
        "MUMEI104",
        "Generated code used while generating",
        "'{0}' only exists in the generated code and can't be used in the generation-time expression '{1}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor GeneratedMethodNotPartial = new(
        "MUMEI105",
        "[Generated] method must be partial",
        "The [Generated] method '{0}' must be declared as 'partial' without a body",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InterceptorMemberIgnored = new(
        "MUMEI106",
        "Interceptor template member is not emitted",
        "'{0}' is not emitted: interceptor templates only emit the body of Intercept. Mark it [CompileTime] to use it while generating.",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}

internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, LocationInfo? Location, string Arg0, string Arg1) {
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxNode? node, string arg0, string arg1 = "") {
        return Create(descriptor, node?.GetLocation(), arg0, arg1);
    }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, Location? location, string arg0, string arg1 = "") {
        var info = location is { IsInSource: true }
            ? new LocationInfo(location.SourceTree!.FilePath, location.SourceSpan, location.GetLineSpan().Span)
            : null;
        return new DiagnosticInfo(descriptor, info, arg0, arg1);
    }

    public Diagnostic ToDiagnostic() {
        var location = Location is { } l ? Microsoft.CodeAnalysis.Location.Create(l.FilePath, l.Span, l.LineSpan) : null;
        return Diagnostic.Create(Descriptor, location, Arg0, Arg1);
    }
}

internal sealed record LocationInfo(string FilePath, TextSpan Span, LinePositionSpan LineSpan);

internal sealed record TemplateResult(string HintName, string? Source, DiagnosticInfo[] Diagnostics) {
    public bool Equals(TemplateResult? other) {
        return other is not null
               && HintName == other.HintName
               && Source == other.Source
               && Diagnostics.SequenceEqual(other.Diagnostics);
    }

    public override int GetHashCode() {
        return HintName.GetHashCode() ^ (Source?.GetHashCode() ?? 0);
    }
}
