using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection;

public static class AssemblySymbolExtensions {
  public static Assembly ToAssembly(this IAssemblySymbol symbol) {
    var modules = symbol.Modules.Select(x => x.ToModule()).ToArray();
    return MakeAssembly(symbol, modules);
  }

  public static Assembly ToAssembly(this IAssemblySymbol symbol, IModuleSymbol callerModule) {
    var modules = symbol.Modules
      .Where(x => x.Name != callerModule.Name)
      .Select(x => x.ToModule())
      .ToArray();

    return MakeAssembly(symbol, modules);
  }

  private static Assembly MakeAssembly(this IAssemblySymbol symbol, Module[] modules) {
    return ReflectionAssembly.Create(symbol.Identity.ToString());
  }
}