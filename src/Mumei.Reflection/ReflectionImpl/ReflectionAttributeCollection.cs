using System.Collections;
using System.Reflection;

namespace Mumei.Common;

public sealed class ReflectionAttributeCollection : IReadOnlyCollection<CustomAttributeData> {
  private readonly CustomAttributeData[] _attributes;

  public ReflectionAttributeCollection(CustomAttributeData[] attributes) {
    _attributes = attributes;
  }

  public IEnumerator<CustomAttributeData> GetEnumerator() {
    return _attributes.AsEnumerable().GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }

  public int Count => _attributes.Length;

  public IList<CustomAttributeData> Clone() {
    var result = new CustomAttributeData[_attributes.Length];
    _attributes.CopyTo(result, 0);
    return result;
  }
}