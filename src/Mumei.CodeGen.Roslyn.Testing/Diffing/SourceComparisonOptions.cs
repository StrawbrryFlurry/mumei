namespace Mumei.CodeGen.Roslyn.Testing;

/// <summary>
/// Controls how two source texts are compared. Line endings are always normalized.
/// </summary>
public sealed record SourceComparisonOptions {
    public static readonly SourceComparisonOptions Default = new();

    /// <summary>How whitespace within a line is treated.</summary>
    public WhitespaceHandling Whitespace { get; init; } = WhitespaceHandling.Normalize;

    /// <summary>
    /// Ignore lines that only contain whitespace. Blank lines at the start and end of a text are
    /// always ignored unless <see cref="Whitespace"/> is <see cref="WhitespaceHandling.Exact"/>.
    /// </summary>
    public bool IgnoreBlankLines { get; init; }

    /// <summary>The number of unchanged lines shown around each difference.</summary>
    public int ContextLines { get; init; } = 3;

    /// <summary>Highlight differences with ANSI colors (they are always also marked with <c>^</c>).</summary>
    public bool UseColors { get; init; } = true;
}

public enum WhitespaceHandling {
    /// <summary>All whitespace is significant.</summary>
    Exact,

    /// <summary>Leading and trailing whitespace of each line (e.g. indentation) is ignored.</summary>
    IgnoreIndentation,

    /// <summary>Like <see cref="IgnoreIndentation"/>, and any run of whitespace within a line equals a single space.</summary>
    Normalize
}
