using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Providers.Registration;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing;
using Mumei.Roslyn.Testing.Comp;
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

  public static readonly CompilationType IWeatherModule =
    $$"""
      {{typeof(ModuleAttribute):Format.Name}}
      public interface {{nameof(IWeatherModule)}} {
        {{typeof(ScopedAttribute<>).Args(WeatherService)}}
        public {{IWeatherService}} WeatherService { get; }
      }
      """;


  [Fact]
  public void ResolveRoot() {
    var c = TestCompilationBuilder.CreateFromSources(IWeatherModule).Build();
    var s = c.GetTypeSymbol(IWeatherModule.Name);
    var r = ModuleLoader.ResolveModule(new RoslynType(s), c);
  }
}