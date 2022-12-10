using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection; 

public static class ModuleSymbolExtensions {
  public static Module ToModule(this IModuleSymbol symbol) {
    var assembly = symbol.ContainingAssembly.ToAssembly(symbol);
    return ReflectionModule.Create(symbol.Name, assembly);
  }
}