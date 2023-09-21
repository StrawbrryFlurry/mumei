using System.Diagnostics.CodeAnalysis;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal struct PartialComponentDeclaration {
  public static bool TryCreateFromAttribute(
    in RoslynAttribute attribute,
    [NotNullWhen(true)] out PartialComponentDeclaration? component
  ) {
    throw new NotImplementedException();
  }
}