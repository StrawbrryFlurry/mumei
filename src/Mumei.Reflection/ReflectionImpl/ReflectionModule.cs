using System.Collections.Concurrent;
using System.Reflection;

namespace Mumei.Common;

public sealed class ReflectionModule : Module {
  private static readonly ConcurrentDictionary<string, ReflectionModule> ModuleCache = new();

  private readonly List<Type> _typesDeclaredInModule = new();

  private ReflectionModule(
    string name,
    Assembly assembly,
    Type[] typesDeclaredInModule) {
    Assembly = assembly;
    Name = name;

    _typesDeclaredInModule.AddRange(typesDeclaredInModule);

    ModuleCache.TryAdd(name, this);
  }

  public override string Name { get; }
  public override Assembly Assembly { get; }

  public static Module Create(
    string name,
    Assembly assembly,
    Type[] typesDeclaredInModule
  ) {
    return ModuleCache.TryGetValue(name, out var module)
      ? module
      : new ReflectionModule(name, assembly, typesDeclaredInModule);
  }
}