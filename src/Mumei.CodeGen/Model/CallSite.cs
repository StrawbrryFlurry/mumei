namespace Mumei.CodeGen;

/// <summary>An equatable snapshot of a method invocation that can be intercepted.</summary>
public sealed record CallSite(MethodModel Method, InterceptLocation InterceptLocation, SourceLocation Location) {
    /// <summary>Whether the call has a receiver (<c>receiver.Method()</c> or an extension method call).</summary>
    public bool HasReceiver => !Method.IsStatic || Method.IsExtension;
}

/// <summary>The data required for <c>[InterceptsLocation]</c>.</summary>
public sealed record InterceptLocation(int Version, string Data, string DisplayLocation) {
    /// <summary><c>global::System.Runtime.CompilerServices.InterceptsLocation(1, "...")</c></summary>
    public string ToAttributeSyntax() {
        return $"global::System.Runtime.CompilerServices.InterceptsLocation({Version}, {CSharpLiteral.String(Data)})";
    }
}

/// <summary>An equatable source location, e.g. for reporting diagnostics.</summary>
public sealed record SourceLocation(
    string FilePath,
    int Start,
    int Length,
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn
) {
    public override string ToString() {
        return $"{FilePath}({StartLine + 1},{StartColumn + 1})";
    }
}
