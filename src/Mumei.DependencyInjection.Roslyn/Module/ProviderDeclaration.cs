using System.Diagnostics.CodeAnalysis;
using Mumei.DependencyInjection.Injector.Registration;
using Mumei.DependencyInjection.Providers.Registration;
using Mumei.Roslyn;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

/// <summary>
/// A single provider specified in a module.
/// <code>
/// interface ISomeModule {
///   [Singleton{SomeImplementation}]
///   public ISomeProvider SomeProvider { get; }
/// }
/// </code>
/// </summary>
internal sealed class ProviderDeclaration : IProviderDeclaration {
  public required RoslynPropertyInfo DeclarationProperty { get; init; }
  public required RoslynType ProviderType { get; init; }
  public required RoslynType ImplementationType { get; init; }
  public required object? ProviderToken { get; init; }

  public required InjectorLifetime ProviderLifetime { get; init; }

  public required bool IsMulti { get; init; }
  public required MultiProviderScope MultiProviderScope { get; init; }

  public static bool TryCreateFromProperty(
    in RoslynPropertyInfo property,
    TemporarySpan<RoslynAttribute> attributes,
    [NotNullWhen(true)] out ProviderDeclaration? specification
  ) {
    var isMultiProvider = false;
    object? customProviderToken = null;
    var scopedProvider = default(ScopedProvider);
    var multiProviderScope = default(MultiProviderScope);

    for (var i = 0; i < attributes.Length; i++) {
      var attribute = attributes[i];
      if (
        TryGetScopedProivder<SingletonAttribute>(
          property,
          attribute,
          typeof(SingletonAttribute<>),
          out var implementationType)
      ) {
        scopedProvider = new ScopedProvider {
          ProviderType = property.Type,
          ImplementationType = implementationType,
          ProviderLifetime = InjectorLifetime.Singleton
        };
        continue;
      }

      if (
        TryGetScopedProivder<ScopedAttribute>(
          property,
          attribute,
          typeof(ScopedAttribute<>),
          out implementationType)
      ) {
        scopedProvider = new ScopedProvider {
          ProviderType = property.Type,
          ImplementationType = implementationType,
          ProviderLifetime = InjectorLifetime.Scoped
        };
        continue;
      }

      if (
        TryGetScopedProivder<TransientAttribute>(
          property,
          attribute,
          typeof(TransientAttribute<>),
          out implementationType)
      ) {
        scopedProvider = new ScopedProvider {
          ProviderType = property.Type,
          ImplementationType = implementationType,
          ProviderLifetime = InjectorLifetime.Transient
        };
        continue;
      }

      if (attribute.Is<MultiAttribute>()) {
        isMultiProvider = true;
        multiProviderScope = MultiProviderScopeCollector.CollectFromAttribute(attribute);
        continue;
      }

      if (attribute.Is<ProvideAttribute>()) {
        customProviderToken = ProviderTokenCollector.CollectFromProvideAttribute(attribute, default);
      }

      // We don't know this attribute, ignore it
    }

    if (scopedProvider.ProviderType.IsDefault()) {
      specification = default;
      return false;
    }

    specification = new ProviderDeclaration {
      DeclarationProperty = property,
      ImplementationType = scopedProvider.ImplementationType,
      ProviderType = scopedProvider.ProviderType,
      ProviderLifetime = scopedProvider.ProviderLifetime,
      ProviderToken = customProviderToken ?? scopedProvider.ProviderType,
      IsMulti = isMultiProvider,
      MultiProviderScope = multiProviderScope
    };
    return true;
  }

  private static bool TryGetScopedProivder<TConstructedGenericType>(
    scoped in RoslynPropertyInfo property,
    scoped in RoslynAttribute attribute,
    Type openProviderType,
    out RoslynType implementationType
  ) where TConstructedGenericType : Attribute {
    if (attribute.Is<TConstructedGenericType>()) {
      implementationType = property.Type;
      return true;
    }

    if (attribute.IsConstructedGenericTypeOf(openProviderType)) {
      implementationType = attribute.Type.GetFirstTypeArgument();
      return true;
    }

    implementationType = default;
    return false;
  }

  private ref struct ScopedProvider {
    public required RoslynType ProviderType { get; init; }
    public required RoslynType ImplementationType { get; init; }

    public required InjectorLifetime ProviderLifetime { get; init; }
  }
}