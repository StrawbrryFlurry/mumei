using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public struct CompilationType {
  private readonly ITypeSymbol _symbol;

  public string Name => _symbol.MetadataName;

  [MemberNotNullWhen(true, nameof(GenericSymbol))]
  public bool IsGenericType => _symbol is INamedTypeSymbol { IsGenericType: true };

  public INamedTypeSymbol? GenericSymbol => _symbol as INamedTypeSymbol;

  public CompilationType(ITypeSymbol symbol) {
    _symbol = symbol;
  }

  public CompilationType GetFirstTypeArgument() {
    if (IsGenericType) {
      return new CompilationType(GenericSymbol.TypeArguments[0]);
    }

    throw new InvalidOperationException("Type is not generic");
  }

  public ReadOnlySpan<CompilationType> GetTypeArguments() {
    if (!IsGenericType) {
      return ReadOnlySpan<CompilationType>.Empty;
    }

    var typeArguments = GenericSymbol.TypeArguments;
    var typeArgumentsBag = new ArrayBuilder<CompilationType>(typeArguments.Length);

    for (var i = 0; i < typeArguments.Length; i++) {
      var typeArgument = typeArguments[i];
      typeArgumentsBag.Add(new CompilationType(typeArgument));
    }

    return typeArgumentsBag.ToReadOnlySpanAndFree();
  }

  public ReadOnlySpan<CompilationAttribute> GetAttributes() {
    var attributes = _symbol.GetAttributes();
    var attributesBag = new ArrayBuilder<CompilationAttribute>(attributes.Length);

    for (var i = 0; i < attributes.Length; i++) {
      var attribute = attributes[i];
      attributesBag.Add(new CompilationAttribute(attribute));
    }

    return attributesBag.ToReadOnlySpanAndFree();
  }

  public string GetFullName() {
    if (_symbol.ContainingNamespace.IsGlobalNamespace && !IsGenericType) {
      return Name;
    }

    var nameBuilder = new ArrayBuilder<char>(Name.Length);

    if (!_symbol.ContainingNamespace.IsGlobalNamespace) {
      AppendNamespaceRecursiveReverse(_symbol.ContainingNamespace, ref nameBuilder);
    }

    nameBuilder.AddRange(Name);

    return nameBuilder.ToStringAndFree();
  }

  private static void AppendNamespaceRecursiveReverse(ISymbol symbol, ref ArrayBuilder<char> nameBuilder) {
    if (symbol.ContainingNamespace is { IsGlobalNamespace: false }) {
      AppendNamespaceRecursiveReverse(symbol.ContainingNamespace, ref nameBuilder);
    }

    nameBuilder.AddRange(symbol.Name.AsSpan());
    nameBuilder.Add('.');
  }
}

public static class CompilationTypeExtensions {
  internal static CompilationType ToCompilationType(this ITypeSymbol typeDeclarationSyntax) {
    return new CompilationType(typeDeclarationSyntax);
  }
}