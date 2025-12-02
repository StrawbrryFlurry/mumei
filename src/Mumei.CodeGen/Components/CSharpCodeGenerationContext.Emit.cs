using System.Collections.Concurrent;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext {
    private readonly CodeGenerationEmitGraph _emitGraph = new();

    public void Emit(string hintName, ISyntheticDeclaration decl) {
        _emitGraph.Track(SyntheticIdentifier.Constant(hintName), decl);
    }

    public void EmitIncremental(string hintName, ISyntheticDeclaration decl) {
        _emitGraph.Track(SyntheticIdentifier.Unique(EmitDeclarationScope.Instance, hintName), decl);
    }

    private sealed class CodeGenerationEmitGraph {
        private readonly ConcurrentDictionary<SyntheticIdentifier, EmitNode> _nodes = new();

        public void Track(SyntheticIdentifier identifier, ISyntheticDeclaration declarationToEmit) {
            var node = _nodes.GetOrAdd(identifier, (_) => new EmitNode());
            node.AddDeclaration(declarationToEmit);
        }

        public CodeGenerationEmitGraph MergeDeclarations(CodeGenerationEmitGraph other) {
            // TODO: Figure out how we want to merge graphs
            return new CodeGenerationEmitGraph();
        }

        public ImmutableArray<(SyntheticIdentifier Name, ImmutableArray<ISyntheticDeclaration> Declarations)> EnumerateDeclarationsToEmit() {
            var results = new ArrayBuilder<(SyntheticIdentifier Name, ImmutableArray<ISyntheticDeclaration> Declarations)>();
            foreach (var entry in _nodes) {
                var trackingName = entry.Key;
                var declarations = entry.Value;

                results.Add((trackingName, declarations.GetDeclarations()));
            }

            return results.ToImmutableArrayAndFree();
        }

        public sealed class EmitNode {
            private readonly ConcurrentDictionary<SyntheticIdentifier, ConcurrentBag<ISyntheticDeclaration>> _declarations = new();

            public void AddDeclaration(ISyntheticDeclaration declaration) {
                var declarationsForId = _declarations.GetOrAdd(declaration.Name, _ => new ConcurrentBag<ISyntheticDeclaration>());
                declarationsForId.Add(declaration);
            }

            public ImmutableArray<ISyntheticDeclaration> GetDeclarations() {
                return _declarations.Values.SelectMany(bag => bag).ToImmutableArray();
            }
        }
    }

    private sealed class EmitDeclarationScope : ISyntheticDeclaration {
        public static readonly EmitDeclarationScope Instance = new();
        public SyntheticIdentifier Name { get; } = SyntheticIdentifier.Constant("<>__EmitScope");
        public ISyntheticDeclaration? Parent { get; } = null;
    }
}