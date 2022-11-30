using System.Collections.Concurrent;
using System.Reflection;

namespace Mumei.Common.Reflection;

public sealed class ReflectionAssembly : Assembly {
  private static readonly ConcurrentDictionary<string, ReflectionAssembly> AssemblyCache  = new();

  private readonly string _name;

  private ReflectionAssembly(string name) {
    _name = name;
    
    AssemblyCache.TryAdd(name, this);
  }

  public static Assembly Create(string name) {
    if (AssemblyCache.TryGetValue(name, out var assembly)) {
      return assembly;
    }
    
    return new ReflectionAssembly(name);
  }
  
  public override string FullName => _name;

  public override Type[] GetExportedTypes() {
    return GetTypes();
  }

  public override Type[] GetTypes() {
    throw new NotImplementedException();
  }
}