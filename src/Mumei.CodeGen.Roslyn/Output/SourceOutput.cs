using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mumei.CodeGen;

/// <summary>
/// Collects everything generated for a single item. Types declared through <see cref="Namespace"/>,
/// <see cref="Class"/>, <see cref="Extend"/>, … end up in one source file; interceptors and
/// <see cref="Source(string,string)"/> files are emitted separately.
/// </summary>
public sealed class SourceOutput {
    private const string Category = "Mumei";

    private static readonly DiagnosticDescriptor GeneratorFailed = new(
        "MUMEI001",
        "Source generation failed",
        "Generating source failed with {0}: {1}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor CallNotInterceptable = new(
        "MUMEI002",
        "Call can't be intercepted",
        "The call to '{0}' can't be intercepted: {1}",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private readonly SourceProductionContext _context;
    private readonly SourceFileBuilder _file = new();
    private readonly List<(string HintName, string Text)> _sources = [];
    private readonly List<Diagnostic> _diagnostics = [];

    private SourceOutput(SourceProductionContext context) {
        _context = context;
    }

    public CancellationToken CancellationToken => _context.CancellationToken;

    /// <summary>The global namespace of the generated file.</summary>
    public NamespaceBuilder Global => _file.Global;

    public NamespaceBuilder Namespace(string name) {
        return _file.Namespace(name);
    }

    /// <summary>Declares a class in the global namespace. Use <see cref="Namespace"/> to declare it elsewhere.</summary>
    public TypeBuilder Class(string name, Modifiers modifiers = Modifiers.Public) {
        return Global.Class(name, modifiers);
    }

    public TypeBuilder Struct(string name, Modifiers modifiers = Modifiers.Public) {
        return Global.Struct(name, modifiers);
    }

    public TypeBuilder Interface(string name, Modifiers modifiers = Modifiers.Public) {
        return Global.Interface(name, modifiers);
    }

    public TypeBuilder Record(string name, Modifiers modifiers = Modifiers.Public) {
        return Global.Record(name, modifiers);
    }

    /// <inheritdoc cref="SourceFileBuilder.Extend"/>
    public TypeBuilder Extend(TypeModel type) {
        return _file.Extend(type);
    }

    /// <summary>
    /// Generates an interceptor for the call. The interceptor is declared in <see cref="InterceptorBuilder.DefaultNamespace"/>
    /// unless configured otherwise; the namespace must be listed in the <c>InterceptorsNamespaces</c> MSBuild property.
    /// </summary>
    public void Intercept(CallSite call, Action<InterceptorBuilder> intercept) {
        var interceptor = new InterceptorBuilder(call);
        if (interceptor.UnsupportedReason is { } reason) {
            Report(Diagnostic.Create(CallNotInterceptable, call.Location.ToLocation(), call.Method.Name, reason));
            return;
        }

        intercept(interceptor);
        _sources.Add((interceptor.HintName, interceptor.ToSourceFile().ToString()));
    }

    /// <summary>Adds a source file verbatim.</summary>
    public void Source(string hintName, string text) {
        _sources.Add((hintName, text));
    }

    /// <inheritdoc cref="Source(string,string)"/>
    public void Source(string hintName, Action<CodeBuilder> build) {
        var code = new CodeBuilder();
        build(code);
        Source(hintName, code.ToString());
    }

    public void Report(Diagnostic diagnostic) {
        _diagnostics.Add(diagnostic);
    }

    public void Error(string id, string message, SourceLocation? location = null) {
        Report(id, message, DiagnosticSeverity.Error, location);
    }

    public void Warning(string id, string message, SourceLocation? location = null) {
        Report(id, message, DiagnosticSeverity.Warning, location);
    }

    public void Info(string id, string message, SourceLocation? location = null) {
        Report(id, message, DiagnosticSeverity.Info, location);
    }

    private void Report(string id, string message, DiagnosticSeverity severity, SourceLocation? location) {
        var descriptor = new DiagnosticDescriptor(id, message, "{0}", Category, severity, isEnabledByDefault: true);
        Report(Diagnostic.Create(descriptor, location?.ToLocation(), message));
    }

    internal static void Run(SourceProductionContext context, Action<SourceOutput> generate) {
        var output = new SourceOutput(context);
        try {
            generate(output);
            output.Flush();
        }
        catch (Exception e) when (e is not OperationCanceledException) {
            context.ReportDiagnostic(Diagnostic.Create(GeneratorFailed, null, e.GetType().Name, e.Message));
        }
    }

    private void Flush() {
        foreach (var diagnostic in _diagnostics) {
            _context.ReportDiagnostic(diagnostic);
        }

        if (!_file.IsEmpty) {
            var text = _file.ToString();
            _sources.Add((SourceFileBuilder.HintName(_file.FirstTypeName ?? "Generated", text), text));
        }

        foreach (var (hintName, text) in _sources) {
            try {
                _context.AddSource(hintName, SourceText.From(text, Encoding.UTF8));
            }
            catch (ArgumentException) when (IsHashed(hintName)) {
                // Another item generated the same file (Mumei's hint names contain a hash of the content or call site),
                // e.g. a helper type shared by many items. Emitting it once is what we want.
            }
        }
    }

    private static bool IsHashed(string hintName) {
        var parts = hintName.Split('.');
        return parts is [.., _, { Length: 16 }, "g", "cs"];
    }
}
