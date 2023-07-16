using System.Collections;

namespace Mumei.DependencyInjection.Providers.Registration;

public sealed class ProviderCollection : IReadOnlyCollection<ProviderDescriptor> {
  private Dictionary<object, ProviderDescriptor> _descriptors = new();

  public int Count => _descriptors.Count;

  public IEnumerator<ProviderDescriptor> GetEnumerator() {
    return _descriptors.Values.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }

  public bool TryGet(object token, out ProviderDescriptor? descriptor) {
    return _descriptors.TryGetValue(token, out descriptor);
  }

  public ProviderCollection Add(ProviderDescriptor descriptor) {
    _descriptors[descriptor.Token] = descriptor;
    return this;
  }
}