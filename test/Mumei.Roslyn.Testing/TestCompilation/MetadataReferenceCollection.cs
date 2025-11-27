using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing;

public sealed class MetadataReferenceCollection {
    private Dictionary<string, MetadataReference> _metadataReferences = new();

    public IReadOnlyCollection<MetadataReference> MetadataReferences =>
        _metadataReferences
            .Select(x => x.Value)
            .ToArray();

    public MetadataReferenceCollection() {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location)) {
                continue;
            }

            _metadataReferences.Add(
                assembly.Location,
                MetadataReference.CreateFromFile(assembly.Location)
            );
        }
    }

    public void AddReference<TAssemblyType>() {
        AddReference(typeof(TAssemblyType));
    }

    public void AddReference(string assemblyName) {
        var assemblyLocation = Assembly.Load(assemblyName).Location;
        if (_metadataReferences.ContainsKey(assemblyLocation)) {
            return;
        }

        _metadataReferences.Add(
            assemblyLocation,
            MetadataReference.CreateFromFile(assemblyLocation)
        );
    }

    public void AddReferences(IEnumerable<Type> assemblyTypes) {
        foreach (var assemblyType in assemblyTypes) {
            AddReference(assemblyType);
        }
    }

    public void AddReferences(ReadOnlySpan<Type> assemblyTypes) {
        foreach (var assemblyType in assemblyTypes) {
            AddReference(assemblyType);
        }
    }

    public void AddReference(Type assemblyType) {
        var assemblyLocation = assemblyType.Assembly.Location;
        if (_metadataReferences.ContainsKey(assemblyLocation)) {
            return;
        }

        _metadataReferences.Add(
            assemblyLocation,
            MetadataReference.CreateFromFile(assemblyLocation)
        );
    }
}