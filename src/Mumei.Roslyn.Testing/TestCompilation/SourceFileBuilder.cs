using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.Roslyn.Testing;

public sealed class SourceFileBuilder {
    private readonly string _content;

    private readonly List<string> _fileComments = new();
    private readonly List<Type> _usings = new();

    public IEnumerable<Type> Usings => _usings;

    public SourceFileBuilder(string content) {
        _content = content;
    }

    public SourceFileBuilder WithUsing<TUsingType>() {
        _usings.Add(typeof(TUsingType));
        return this;
    }

    public SourceFileBuilder WithFileComment(string comment) {
        _fileComments.Add(comment);
        return this;
    }

    internal SyntaxTree ToSyntaxTree() {
        return CSharpSyntaxTree.ParseText(ToString());
    }

    public override string ToString() {
        var sourceText = new StringBuilder();

        if (_fileComments.Count > 0) {
            foreach (var comment in _fileComments) {
                sourceText.AppendLine($"// {comment}");
            }

            sourceText.AppendLine();
        }

        if (_usings.Count > 0) {
            var uniqueNamespaces = _usings
                .Select(x => x.Namespace)
                .Distinct()
                .Select(x => $"using {x};");

            foreach (var ns in uniqueNamespaces) {
                sourceText.AppendLine(ns);
            }

            sourceText.AppendLine();
        }

        sourceText.Append(_content);

        return sourceText.ToString();
    }
}