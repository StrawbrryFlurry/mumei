using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn.RoslynCodeProviders;

internal sealed class CompilationHostOutputIdentifierResolver : IIdentifierResolver {
    private readonly Dictionary<object, UniqueNameGeneratorComponent> _generators = new();
    private UniqueNameGeneratorComponent? _globalGenerator;

    public string MakeGloballyUnique(string name) {
        _globalGenerator ??= new UniqueNameGeneratorComponent();
        return _globalGenerator.MakeUnique(name);
    }

    public string MakeUnique(object scope, string name) {
        if (!_generators.TryGetValue(scope, out var generator)) {
            generator = new UniqueNameGeneratorComponent();
            _generators[scope] = generator;
        }

        return generator.MakeUnique(name);
    }
}