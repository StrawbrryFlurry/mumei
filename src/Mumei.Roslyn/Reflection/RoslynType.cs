using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public readonly struct RoslynType {
  private readonly ITypeSymbol _symbol;

  public string Name => _symbol.MetadataName;

  [MemberNotNullWhen(true, nameof(GenericSymbol))]
  public bool IsGenericType => _symbol is INamedTypeSymbol { IsGenericType: true };

  public INamedTypeSymbol? GenericSymbol => _symbol as INamedTypeSymbol;

  public RoslynType(ITypeSymbol symbol) {
    _symbol = symbol;
  }

  public RoslynType GetFirstTypeArgument() {
    if (IsGenericType) {
      return new RoslynType(GenericSymbol.TypeArguments[0]);
    }

    throw new InvalidOperationException("Type is not generic");
  }

  public TemporarySpan<RoslynType> GetTypeArgumentsTemp() {
    return GetTypeArgumentsBag().AsRefForfeitOwnership();
  }

  public ReadOnlySpan<RoslynType> GetTypeArguments() {
    return GetTypeArgumentsBag().ToReadOnlySpanAndFree();
  }

  private ArrayBuilder<RoslynType> GetTypeArgumentsBag() {
    if (!IsGenericType) {
      return ArrayBuilder<RoslynType>.Empty;
    }

    var typeArguments = GenericSymbol.TypeArguments;
    var typeArgumentsBag = new ArrayBuilder<RoslynType>(typeArguments.Length);

    for (var i = 0; i < typeArguments.Length; i++) {
      var typeArgument = typeArguments[i];
      typeArgumentsBag.Add(new RoslynType(typeArgument));
    }

    return typeArgumentsBag;
  }

  public TemporarySpan<RoslynAttribute> GetAttributesTemp() {
    return RoslynAttributeCollector.GetAttributesTemp(_symbol);
  }

  public ReadOnlySpan<RoslynAttribute> GetAttributes() {
    return RoslynAttributeCollector.GetAttributes(_symbol);
  }

  public TemporarySpan<RoslynPropertyInfo> GetPropertiesTemp() {
    return GetPropertiesBag().AsRefForfeitOwnership();
  }

  public ReadOnlySpan<RoslynPropertyInfo> GetProperties() {
    return GetPropertiesBag().ToReadOnlySpanAndFree();
  }

  private ArrayBuilder<RoslynPropertyInfo> GetPropertiesBag() {
    var members = _symbol.GetMembers();
    if (members.Length == 0) {
      return ArrayBuilder<RoslynPropertyInfo>.Empty;
    }

    var m = members.AsSpan();
    var propertiesBag = ArrayBuilder<RoslynPropertyInfo>.CreateWithApproximateSize(members.Length);
    for (var i = 0; i < m.Length; i++) {
      var member = m[i];
      if (member is IPropertySymbol ps) {
        propertiesBag.Add(new RoslynPropertyInfo(ps));
      }
    }

    return propertiesBag;
  }

  public TemporarySpan<RoslynMethodInfo> GetMethodsTemp() {
    return GetMethodsBag().AsRefForfeitOwnership();
  }

  public ReadOnlySpan<RoslynMethodInfo> GetMethods() {
    return GetMethodsBag().ToReadOnlySpanAndFree();
  }

  private ArrayBuilder<RoslynMethodInfo> GetMethodsBag() {
    var members = _symbol.GetMembers();
    if (members.Length == 0) {
      return ArrayBuilder<RoslynMethodInfo>.Empty;
    }

    var methodsBag = ArrayBuilder<RoslynMethodInfo>.CreateWithApproximateSize(members.Length);
    var m = members.AsSpan();
    for (var i = 0; i < m.Length; i++) {
      var method = m[i];
      if (method is IMethodSymbol { MethodKind: MethodKind.Ordinary } ms) {
        methodsBag.Add(new RoslynMethodInfo(ms));
      }
    }

    return methodsBag;
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

  public static bool operator ==(RoslynType left, RoslynType right) {
    return left.Equals(right);
  }

  public static bool operator !=(RoslynType left, RoslynType right) {
    return !(left == right);
  }

  public override int GetHashCode() {
#pragma warning disable RS1024
    return _symbol.GetHashCode();
#pragma warning restore RS1024
  }

  public override bool Equals(object? obj) {
    return obj is RoslynType other && Equals(other);
  }

  public bool Equals(RoslynType other) {
    return SymbolEqualityComparer.Default.Equals(_symbol, other._symbol);
  }
}