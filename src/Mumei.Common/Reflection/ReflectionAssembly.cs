using System.Collections.Concurrent;
using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class ReflectionAssembly : Assembly {
  private static readonly ConcurrentDictionary<string, ReflectionAssembly> AssemblyCache = new();

  private ReflectionAssembly(string name) {
    FullName = name;

    AssemblyCache.TryAdd(name, this);
  }

  public override string FullName { get; }

  public static Assembly Create(string name) {
    return AssemblyCache.GetOrAdd(
      name,
      _ => new ReflectionAssembly(name)
    );
  }

  public override Type[] GetExportedTypes() {
    return GetTypes();
  }

  public override Type[] GetTypes() {
    var typesInAssembly = new List<Type>();
    foreach (var module in Modules) {
      var typesInModule = module.GetTypes();
      typesInAssembly.AddRange(typesInModule);
    }

    return typesInAssembly.ToArray();
  }
}