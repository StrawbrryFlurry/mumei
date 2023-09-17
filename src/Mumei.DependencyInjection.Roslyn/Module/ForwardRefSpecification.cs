using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

internal sealed class ForwardRefSpecification {
  public ModuleDeclaration Parent { get; }

  public static bool TryCreateFromProperty(
    PropertyInfo property,
    [NotNullWhen(true)] out ForwardRefSpecification? spec
  ) {
    spec = default!;
    return false;
  }
}