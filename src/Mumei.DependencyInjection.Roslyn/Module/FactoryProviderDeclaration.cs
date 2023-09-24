using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Mumei.DependencyInjection.Injector.Registration;
using Mumei.DependencyInjection.Providers.Registration;
using Mumei.Roslyn;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn.Module;

public sealed class FactoryProviderDeclaration : IProviderDeclaration {
  public required ImmutableArray<RoslynType> Dependencies { get; init; }
  public required object? ProviderToken { get; init; }
  public required RoslynType ProviderType { get; init; }
  public required RoslynMethodInfo FactoryMethod { get; init; }

  public required InjectorLifetime Lifetime { get; init; }
  public required MultiProviderScope MultiProviderScope { get; init; }

  public required bool IsMulti { get; init; }

  public static bool TryCreateFromMethod(
    in RoslynMethodInfo method,
    in TemporarySpan<RoslynAttribute> attributes,
    [NotNullWhen(true)] out FactoryProviderDeclaration? factoryProvider
  ) {
    InjectorLifetime? lifetime = null;
    object? proivderToken = null;
    var isMulti = false;
    var multiProviderScope = default(MultiProviderScope);

    for (var i = 0; i < attributes.Length; i++) {
      var attribute = attributes[i];
      var foundMatch = attribute switch {
        _ when attribute.Is<ProvideSingletonAttribute>() => lifetime = InjectorLifetime.Singleton,
        _ when attribute.Is<ProvideScopedAttribute>() => lifetime = InjectorLifetime.Scoped,
        _ when attribute.Is<ProvideTransientAttribute>() => lifetime = InjectorLifetime.Transient,
        _ when attribute.Is<ProvideAttribute>() => proivderToken =
          ProviderTokenCollector.CollectFromProvideAttribute(attribute, default),
        _ => false // Ignore unknown attributes
      };

      if (foundMatch is not false) {
        continue;
      }

      if (attribute.Is<MultiAttribute>()) {
        isMulti = true;
        multiProviderScope = MultiProviderScopeCollector.CollectFromAttribute(attribute);
      }
    }

    if (lifetime is null) {
      factoryProvider = null;
      return false;
    }

    factoryProvider = new FactoryProviderDeclaration {
      Lifetime = lifetime.Value,
      ProviderToken = proivderToken,
      ProviderType = method.ReturnType,
      FactoryMethod = method,
      Dependencies = method.GetParameters(),
      IsMulti = isMulti,
      MultiProviderScope = multiProviderScope
    };
    return true;
  }
}