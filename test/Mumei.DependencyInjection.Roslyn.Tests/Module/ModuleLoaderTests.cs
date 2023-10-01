using FluentAssertions;
using Mumei.DependencyInjection.Injector.Registration;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.DependencyInjection.Providers.Dynamic.Registration;
using Mumei.DependencyInjection.Providers.Registration;
using Mumei.DependencyInjection.Roslyn.Module;
using Mumei.Roslyn.Testing.Comp;
using Mumei.Roslyn.Testing.FluentAssertions;
using Mumei.Roslyn.Testing.Template;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Mumei.DependencyInjection.Roslyn.Tests.Module;

public sealed class ModuleLoaderTests {
  private static readonly CompilationType IWeatherService =
    $$"""
      public interface {{nameof(IWeatherService)}} {
        public string GetWeather();
      }
      """;

  private static readonly CompilationType WeatherService =
    $$"""
      public class {{nameof(WeatherService)}} : {{IWeatherService}} {
        public string GetWeather() => "Sunny";
      }
      """;

  private static readonly CompilationType IWeatherModule =
    $$"""
      {{typeof(ModuleAttribute)}}
      public interface {{nameof(IWeatherModule)}} {
        {{typeof(ScopedAttribute<>).Args(WeatherService)}}
        public {{IWeatherService}} WeatherService { get; }
      }
      """;

  [Fact]
  public void ResolveRootModule_ShouldSetDeclaringTypeToProvidedTypeSymbol() {
    var rootSymbol = TestCompilation.CompileTypeSymbol(IWeatherModule, out var c);

    var rootModule = ModuleLoader.ResolveRootModule(rootSymbol, c);

    rootModule.DeclaringType.Should().BeSymbol(c.GetTypeSymbol(IWeatherModule));
  }

  [Fact]
  public void ResolveRootModule_ShouldReturnModuleDeclarationWithCorrectProviderDeclaration() {
    var rootSymbol = TestCompilation.CompileTypeSymbol(IWeatherModule, out var c);

    var rootModule = ModuleLoader.ResolveRootModule(rootSymbol, c);

    rootModule.Providers.Should().HaveCount(1);
    var provider = rootModule.Providers.Single() as ProviderDeclaration;
    provider.ProviderType.Should().BeSymbol(c.GetTypeSymbol(IWeatherService));
    provider.ProviderLifetime.Should().Be(InjectorLifetime.Scoped);
    provider.ImplementationType.Should().BeSymbol(c.GetTypeSymbol(WeatherService));
  }

  private static readonly CompilationType IApplicationModule =
    $$"""
      {{typeof(ModuleAttribute)}}
      {{typeof(ImportAttribute<>).Args(IWeatherModule)}}
      public interface {{nameof(IApplicationModule)}} {
      }
      """;

  [Fact]
  public void ResolveRootModule_ShouldReturnModuleDeclarationWithRecursiveImportDeclarations() {
    var rootSymbol = TestCompilation.CompileTypeSymbol(IApplicationModule, out var c);

    var rootModule = ModuleLoader.ResolveRootModule(rootSymbol, c);

    rootModule.Imports.Should().HaveCount(1);
    var import = rootModule.Imports.Single();
    import.DeclaringType.Should().BeSymbol(c.GetTypeSymbol(IWeatherModule));
    import.Providers.Should().HaveCount(1);
    import.Providers[0].ProviderType.Should().BeSymbol(c.GetTypeSymbol(IWeatherService));
  }

  private static readonly CompilationType IScopedProvider =
    $$"""
      public interface {{nameof(IScopedProvider)}} { }
      """;

  private static readonly CompilationType ISingletonProvider =
    $$"""
      public interface {{nameof(ISingletonProvider)}} { }
      """;

  private static readonly CompilationType ITransientProvider =
    $$"""
      public interface {{nameof(ITransientProvider)}} { }
      """;

  private static readonly CompilationType IForwardRefProvider =
    $$"""
      public interface {{nameof(IForwardRefProvider)}} { }
      """;

  private static readonly CompilationType IFactoryProvider =
    $$"""
      public interface {{nameof(IFactoryProvider)}} { }
      """;

  private static readonly CompilationType FactoryProviderImpl =
    $$"""
      public class {{nameof(FactoryProviderImpl)}} : {{IFactoryProvider}} { }
      """;

  private static readonly CompilationType IConfigurableProvider =
    $$"""
      public interface {{nameof(IConfigurableProvider)}} {
        public string BaseUrl { get; set; }
      }
      """;

  private static readonly CompilationType IConfigurableForProvider =
    $$"""
      public interface {{nameof(IConfigurableForProvider)}} {
        public string BaseUrl { get; set; }
      }
      """;

  private static readonly CompilationType MediatRProviderBinder =
    $$"""
      public class {{nameof(MediatRProviderBinder)}} : {{typeof(IProviderBinder)}}{
        public void Bind({{typeof(ProviderCollection)}} providers) {
        }
      }
      """;

  private static readonly CompilationType IComplexModule =
    $$"""
      {{typeof(ModuleAttribute)}}
      {{typeof(DynamicallyBindAttribute<>).Args(MediatRProviderBinder)}}
      public interface {{nameof(IComplexModule)}} {
        {{typeof(ScopedAttribute<>).Args(IScopedProvider)}}
        public {{IScopedProvider}} ScopedProvider { get; }
      
        {{typeof(SingletonAttribute<>).Args(ISingletonProvider)}}
        public {{ISingletonProvider}} SingletonProvider { get; }
      
        {{typeof(TransientAttribute<>).Args(ITransientProvider)}}
        public {{ITransientProvider}} TransientProvider { get; }
      
        {{typeof(UseExistingAttribute<>).Args(IScopedProvider)}}
        public {{IScopedProvider}} OtherScopedProvider { get; }
      
        {{typeof(ForwardRefAttribute)}}
        public {{IForwardRefProvider}} ForwardRefProvider { get; }
      
        {{typeof(ProvideScopedAttribute)}}
        public {{IFactoryProvider}} CreateFactoryProvider() {
          return new {{FactoryProviderImpl}}();
        }
      
        {{typeof(ConfigureAttribute)}}
        public void ConfigureProvider({{IConfigurableProvider}} provider) {
          provider.BaseUrl = "https://localhost:5001";
        }
        
        {{typeof(ConfigureForAttribute<>).Args(IScopedProvider)}}
        public void ConfigureProvider({{IConfigurableForProvider}} provider) {
          provider.BaseUrl = "https://localhost:5001";
        }
      }
      """;

  [Fact]
  public void ResolveRootModule_ShouldReturnModuleDeclarationWithCorrectProviders_WhenModuleHasScopedProvider() {
    var rootSymbol = TestCompilation.CompileTypeSymbol(IComplexModule, out var c);

    var rootModule = ModuleLoader.ResolveRootModule(rootSymbol, c);

    rootModule.Providers.Should().Contain(x => x.ProviderType == c.GetTypeSymbol(IScopedProvider));
    var p = rootModule.Providers.Single(x => x.ProviderType == c.GetTypeSymbol(IScopedProvider)) as ProviderDeclaration;
    p.ProviderLifetime.Should().Be(InjectorLifetime.Scoped);
    p.ImplementationType.Should().BeSymbol(c.GetTypeSymbol(IScopedProvider));
  }

  [Fact]
  public void ResolveRootModule_ShouldReturnModuleDeclarationWithCorrectProviders_WhenModuleHasSingletonProvider() {
    var rootSymbol = TestCompilation.CompileTypeSymbol(IComplexModule, out var c);

    var rootModule = ModuleLoader.ResolveRootModule(rootSymbol, c);

    rootModule.Providers.Should().Contain(x => x.ProviderType == c.GetTypeSymbol(ISingletonProvider));
    var p =
      rootModule.Providers.Single(x => x.ProviderType == c.GetTypeSymbol(ISingletonProvider)) as ProviderDeclaration;
    p.ProviderLifetime.Should().Be(InjectorLifetime.Singleton);
    p.ImplementationType.Should().BeSymbol(c.GetTypeSymbol(ISingletonProvider));
  }

  [Fact]
  public void ResolveRootModule_ShouldReturnModuleDeclarationWithCorrectProviders_WhenModuleHasTransientProvider() {
    var rootSymbol = TestCompilation.CompileTypeSymbol(IComplexModule, out var c);

    var rootModule = ModuleLoader.ResolveRootModule(rootSymbol, c);

    rootModule.Providers.Should().Contain(x => x.ProviderType == c.GetTypeSymbol(ITransientProvider));
    var p =
      rootModule.Providers.Single(x => x.ProviderType == c.GetTypeSymbol(ITransientProvider)) as ProviderDeclaration;
    p.ProviderLifetime.Should().Be(InjectorLifetime.Transient);
    p.ImplementationType.Should().BeSymbol(c.GetTypeSymbol(ITransientProvider));
  }

  [Fact]
  public void ResolveRootModule_ShouldReturnModuleDeclarationWithCorrectProviders_WhenModuleHasUseExistingProvider() {
    var rootSymbol = TestCompilation.CompileTypeSymbol(IComplexModule, out var c);

    var rootModule = ModuleLoader.ResolveRootModule(rootSymbol, c);

    rootModule.Providers.Should().ContainSingle(x => x is ForwardRefDeclaration);
    var p = rootModule.Providers.Single(x => x is ForwardRefDeclaration) as ForwardRefDeclaration;
    p.ProviderType.Should().BeSymbol(c.GetTypeSymbol(IForwardRefProvider));
    p.Source.Should().Be(ForwardRefSource.Parent);
  }


  // Global Module
  // Components
}