using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

public static class IncrementalGeneratorSourceOutputExtensions { }

public static class SyntaxProviderExtensions {
    public static void RegisterQtSourceOutput<TMatchedNode>(
        this IncrementalGeneratorInitializationContext context,
        Func<NodeMatchingContext, NodeMatchResult<TMatchedNode>> nodeMatcher,
        Action<OutputGenerationContext<TMatchedNode>> outputGenerator
    ) {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            nodeMatcher.UserNodeMatcherToPredicate(),
            outputGenerator.UserOutputGeneratorToTransform(nodeMatcher)
        ).Where(x => !x.IsEmpty);

        context.RegisterSourceOutput(provider, (ctx, p) => {
            foreach (var diagnostic in p.Diagnostics) {
                ctx.ReportDiagnostic(diagnostic);
            }

            foreach (var (fileName, text) in p.GeneratedFiles) {
                ctx.AddSource(fileName, text);
            }
        });
    }

    public static void RegisterQtSourceOutput<TValues>(
        this IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<TValues> provider,
        Func<TValues, SemanticModel> semanticModelProvider,
        Action<OutputGenerationContext<TValues>> outputGenerator
    ) {
        context.RegisterSourceOutput(provider, (ctx, p) => {
            var sm = semanticModelProvider(p);
            var outputContext = new OutputGenerationContext<TValues> {
                State = p,
                CancellationToken = ctx.CancellationToken,
                SemanticModel = sm
            };

            outputGenerator(outputContext);
            var output = outputContext.CreateOutput();

            foreach (var diagnostic in output.Diagnostics) {
                ctx.ReportDiagnostic(diagnostic);
            }

            foreach (var (fileName, text) in output.GeneratedFiles) {
                ctx.AddSource(fileName, text);
            }
        });
    }

    private static Func<SyntaxNode, CancellationToken, bool> UserNodeMatcherToPredicate<TNodeMatch>(
        this Func<NodeMatchingContext, NodeMatchResult<TNodeMatch>> nodeMatcher
    ) {
        return (node, ct) => {
            var ctx = new NodeMatchingContext { Node = node, CancellationToken = ct };
            var match = nodeMatcher(ctx);
            return match.Exists;
        };
    }

    private static Func<GeneratorSyntaxContext, CancellationToken, CodeGenerationOutput> UserOutputGeneratorToTransform<TNodeMatch>(
        this Action<OutputGenerationContext<TNodeMatch>> userTransform,
        Func<NodeMatchingContext, NodeMatchResult<TNodeMatch>> nodeMatcher
    ) {
        return (context, ct) => {
            var nodeMatch = nodeMatcher(new NodeMatchingContext { Node = context.Node, CancellationToken = ct });
            Debug.Assert(nodeMatch.Exists);
            QtCompilationScope.SetActiveScope(context.SemanticModel.Compilation);
            var ctx = new OutputGenerationContext<TNodeMatch> {
                State = nodeMatch.Match!,
                CancellationToken = ct,
                SemanticModel = context.SemanticModel
            };

            userTransform(ctx);

            return ctx.CreateOutput();
        };
    }
}

public readonly struct NodeMatchResult {
    public static readonly NoneResult None = new();
    public readonly struct NoneResult { }

    public static NodeMatchResult<T> Of<T>(T match) {
        return new NodeMatchResult<T> { Match = match };
    }
}

public readonly struct NodeMatchResult<T> {
    public required T? Match { get; init; }

    public bool Exists => Match is not null && !Match.Equals(default(T));

    public static implicit operator NodeMatchResult<T>(T? match) {
        return new NodeMatchResult<T> { Match = match };
    }

    public static implicit operator NodeMatchResult<T>(NodeMatchResult.NoneResult noneResult) {
        return new NodeMatchResult<T> { Match = default };
    }
}

public readonly struct NodeMatchingContext {
    public required CancellationToken CancellationToken { get; init; }
    public required SyntaxNode Node { get; init; }
}

public readonly struct OutputGenerationContext<TNodeMatch>() {
    // Lets us track mutable state in a readonly struct
    private readonly OutputGenerationContextState _state = new();

    public required CancellationToken CancellationToken { get; init; }
    public required SemanticModel SemanticModel { get; init; }
    public required TNodeMatch State { get; init; }

    public static implicit operator CancellationToken(OutputGenerationContext<TNodeMatch> ctx) {
        return ctx.CancellationToken;
    }

    public static implicit operator SemanticModel(OutputGenerationContext<TNodeMatch> ctx) {
        return ctx.SemanticModel;
    }

    private sealed class OutputGenerationContextState {
        public QtCollection<Diagnostic> Diagnostics;
        public QtCollection<QtSourceFile> GeneratedFiles;

        internal void AddDiagnostic(Diagnostic diagnostic) {
            Diagnostics = Diagnostics.Add(diagnostic);
        }

        internal void AddGeneratedFile(QtSourceFile file) {
            GeneratedFiles = GeneratedFiles.Add(file);
        }
    }

    internal unsafe CodeGenerationOutput CreateOutput() {
        if (_state.GeneratedFiles.IsEmpty && _state.Diagnostics.IsEmpty) {
            return new CodeGenerationOutput { GeneratedFiles = [], Diagnostics = [] };
        }

        var files = _state.GeneratedFiles.Select(file => {
            var renderTree = new SourceFileRenderTreeBuilder();
            using var ctx = TemplateBindingContext.StartBinding();
            file.Render(renderTree);
            // ctx.CodeGenFeatures.WriteSourceFileFeatures(ref renderTree);
            var text = renderTree.GetSourceText();
            return (file.Name, SourceText.From(text, Encoding.UTF8));
        });

        return new CodeGenerationOutput {
            Diagnostics = _state.Diagnostics.Span.ToArray(),
            GeneratedFiles = files.Span.ToArray()
        };
    }

    public void AddFile(in QtSourceFile file) {
        _state.AddGeneratedFile(file);
    }
}

internal sealed class CodeGenerationOutput {
    public bool IsEmpty => Diagnostics.Length == 0 && GeneratedFiles.Length == 0;

    internal required Diagnostic[] Diagnostics { get; init; }
    internal required (string FileName, SourceText Text)[] GeneratedFiles { get; init; }
}