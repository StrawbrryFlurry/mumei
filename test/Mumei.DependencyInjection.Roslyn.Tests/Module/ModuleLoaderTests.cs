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
      {{"Weather":namespace}}

      public interface {{nameof(IWeatherService)}} {
        public string GetWeather();
      }
      """;

  private static readonly CompilationType WeatherService =
    $$"""
      {{"Weather":namespace}}

      public class {{nameof(WeatherService)}} : {{IWeatherService}} {
        public string GetWeather() => "Sunny";
      }
      """;

  private static readonly CompilationType IWeatherModule =
    $$"""
      {{"Weather":namespace}}

      {{typeof(ModuleAttribute)}}
      public interface {{nameof(IWeatherModule)}} {
        {{typeof(ScopedAttribute<>).Args(WeatherService)}}
        public {{IWeatherService}} WeatherService { get; }
      }
      """;

  [Fact]
  public void Boop() {
    var s = TestCompilation.CompileTypeSymbol(IWeatherModule, out var c);
    var r = ModuleLoader.ResolveModule(new RoslynType(s), c);
  }
}