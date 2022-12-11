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

  public override string Name { get; }
  public override Assembly Assembly { get; }

  public static Module Create(string name, Assembly assembly) {
    return ModuleCache.TryGetValue(name, out var module)
      ? module
      : new ReflectionModule(name, assembly);
  }
}