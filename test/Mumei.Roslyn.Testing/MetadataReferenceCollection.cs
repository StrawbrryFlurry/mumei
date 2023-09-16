using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing;

public sealed class MetadataReferenceCollection {
  private Dictionary<string, MetadataReference> _metadataReferences = new();

  public IReadOnlyCollection<MetadataReference> MetadataReferences =>
    _metadataReferences
      .Select(x => x.Value)
      .ToArray();

  public MetadataReferenceCollection() {
    _metadataReferences.Add(
      MetadataReferenceCache.SystemCoreLib.AssemblyLocation,
      MetadataReferenceCache.SystemCoreLib.MetadataReference
    );
  }

  public void AddReference<TAssemblyType>() {
    AddReference(typeof(TAssemblyType));
  }

  public void AddReferences(IEnumerable<Type> assemblyTypes) {
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