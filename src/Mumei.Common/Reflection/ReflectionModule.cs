using System.Collections.Concurrent;
using System.Reflection;

namespace Mumei.Common.Reflection; 

public sealed class ReflectionModule : Module {
  private static readonly ConcurrentDictionary<string, ReflectionModule> ModuleCache = new();

  private ReflectionModule(string name, Assembly assembly) {
    Assembly = assembly;
    Name = name;

    ModuleCache.TryAdd(name, this);
  }

  public static Module Create(string name, Assembly assembly) {
    if (ModuleCache.TryGetValue(name, out var module)) {
      return module;
    }

    return new ReflectionModule(name, assembly);
  }
  
  public override string Name { get; }
  public override Assembly Assembly { get; }
}