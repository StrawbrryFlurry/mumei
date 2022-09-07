namespace Mumei.Core.Attributes;

/// <summary>
///   Enables a provider token (Type, provider name, etc.)
///   to provide multiple values / instances. Depending on the
///   resolution strategy, mumei will try to generate the best possible
///   path to resolve all instances of the provider. Note that all
///   providers with that token will need to specify the same resolution
///   strategy and be marked as multi. If there is a multi provider with a
///   different resolution strategy in parent / sibling module, the closest
///   provider in the injector tree will be used. Providers will only be merged,
///   when they share both the same token and resolution strategy, given that the
///   provider to be merged is contained in the scope of the resolution strategy.
/// </summary>
public class MultiAttribute : Attribute {
  public MultiAttribute(MultiProviderResolutionStrategy strategy = MultiProviderResolutionStrategy.Module) {
    Strategy = strategy;
  }

  public MultiProviderResolutionStrategy Strategy { get; }
}

public enum MultiProviderResolutionStrategy {
  /// <summary>
  ///   All providers with the same token in this module
  ///   will be grouped together as a multi provider.
  /// </summary>
  Module,

  /// <summary>
  ///   The provider will be grouped with all other providers
  ///   from imports that are marked with the same multi token.
  /// </summary>
  Imports,

  /// <summary>
  ///   The provider will be grouped with all other app multi
  ///   provider tokens within this application module.
  /// </summary>
  App
}