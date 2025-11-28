using System.Collections.Immutable;
using Mumei.CodeGen.Rendering;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext : ICodeGenerationContext {
    private readonly Dictionary<string, Dictionary<string, ISyntheticNamespace>> _namespacesToEmit = new();
    private readonly Dictionary<Type, object> _synthesisProviders = new();

    public T? Synthesize<T>(object? constructable, T? defaultValue = default) {
        if (constructable is ISyntheticConstructable<T> c) {
            var ctx = new CSharpCompilationUnitContext(this);
            return c.Construct(ctx);
        }

        if (!defaultValue?.Equals(default(T)) ?? true) {
            return defaultValue;
        }

        throw new NotSupportedException();
    }

    public T? SynthesizeOptional<T>(object? constructable) {
        if (constructable is ISyntheticConstructable<T> c) {
            var ctx = new CSharpCompilationUnitContext(this);
            return c.Construct(ctx);
        }

        return default;
    }

    public void Emit(string hintName, ISyntheticNamespace ns) {
        if (!_namespacesToEmit.TryGetValue(hintName, out var existingNamespaceMap)) {
            _namespacesToEmit[hintName] = new Dictionary<string, ISyntheticNamespace> {
                [ns.FullyQualifiedName] = ns
            };
            return;
        }

        if (!existingNamespaceMap.TryGetValue(ns.FullyQualifiedName, out var existingMatchingNamespace)) {
            existingNamespaceMap[ns.FullyQualifiedName] = ns;
            return;
        }

        foreach (var member in ns.Members) {
            existingMatchingNamespace.WithMember(member);
        }
    }

    public void RegisterSynthesisProvider<TSynthesizer>(TSynthesizer synthesizer) where TSynthesizer : ISynthesisProvider {
        _synthesisProviders[typeof(TSynthesizer)] = synthesizer;
    }

    public TProvider GetSynthesisProvider<TProvider>() where TProvider : ISynthesisProvider {
        if (_synthesisProviders.TryGetValue(typeof(TProvider), out var provider)) {
            return (TProvider) provider;
        }

        throw new InvalidOperationException($"No synthesis provider of type {typeof(TProvider)} has been registered.");
    }
}

internal sealed class CSharpCompilationUnitContext(ICodeGenerationContext codeGenContext) : ICompilationUnitContext {
    public ICodeGenerationContext CodeGenContext => codeGenContext;

    public T? Synthesize<T>(object? constructable, T? defaultValue = default) {
        return CodeGenContext.Synthesize(constructable, defaultValue);
    }

    public T? SynthesizeOptional<T>(object? constructable) {
        return CodeGenContext.SynthesizeOptional<T>(constructable);
    }
}