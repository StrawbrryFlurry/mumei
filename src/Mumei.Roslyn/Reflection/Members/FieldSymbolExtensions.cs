using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection.Members;

namespace Mumei.Roslyn.Reflection.Members; 

public static class FieldSymbolExtensions {
  public static FieldInfoSpec ToFieldInfoSpec(this IFieldSymbol fieldSymbol) {
    return new FieldInfoSpec();
  }
}
