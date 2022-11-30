using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection; 

public static class AssemblySymbolExtensions {
  public static Assembly ToAssembly(this IAssemblySymbol symbol) {
    var modules = symbol.Modules.Select(x => x.ToModule()).ToArray();
    return ReflectionAssembly.Create("");
  }
}