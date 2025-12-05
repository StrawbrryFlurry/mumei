using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mumei.CodeGen.Roslyn;

public sealed class CSharpRendererSyntaxTree : SyntaxTree {
    public override bool TryGetText([NotNullWhen(true)] out SourceText? text) {
        throw new NotImplementedException();
    }
    public override SourceText GetText(CancellationToken cancellationToken = new()) {
        throw new NotImplementedException();
    }
    protected override bool TryGetRootCore([NotNullWhen(true)] out SyntaxNode? root) {
        throw new NotImplementedException();
    }
    protected override SyntaxNode GetRootCore(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
    protected override Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
    public override SyntaxTree WithChangedText(SourceText newText) {
        throw new NotImplementedException();
    }
    public override IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = new()) {
        throw new NotImplementedException();
    }
    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node) {
        throw new NotImplementedException();
    }
    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token) {
        throw new NotImplementedException();
    }
    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia) {
        throw new NotImplementedException();
    }
    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken) {
        throw new NotImplementedException();
    }
    public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = new()) {
        throw new NotImplementedException();
    }
    public override FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = new()) {
        throw new NotImplementedException();
    }
    public override IEnumerable<LineMapping> GetLineMappings(CancellationToken cancellationToken = new()) {
        throw new NotImplementedException();
    }
    public override bool HasHiddenRegions() {
        throw new NotImplementedException();
    }
    public override IList<TextSpan> GetChangedSpans(SyntaxTree syntaxTree) {
        throw new NotImplementedException();
    }
    public override Location GetLocation(TextSpan span) {
        throw new NotImplementedException();
    }
    public override bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false) {
        throw new NotImplementedException();
    }
    public override SyntaxReference GetReference(SyntaxNode node) {
        throw new NotImplementedException();
    }
    public override IList<TextChange> GetChanges(SyntaxTree oldTree) {
        throw new NotImplementedException();
    }
    public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options) {
        throw new NotImplementedException();
    }
    public override SyntaxTree WithFilePath(string path) {
        throw new NotImplementedException();
    }
    public override string FilePath { get; }
    public override bool HasCompilationUnitRoot { get; }
    protected override ParseOptions OptionsCore { get; }
    public override int Length { get; }
    public override Encoding? Encoding { get; }
}