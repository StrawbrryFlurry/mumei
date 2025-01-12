using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.Roslyn.Testing;
using Xunit;

namespace Mumei.DependencyInjection.Roslyn.Tests;

public sealed class SampleIncrementalSourceGeneratorTests {
  private const string VectorClassText =
    """
    [Module]
    public partial interface IWeatherModule {
    }
    """;

  private const string ExpectedGeneratedClassText =
    """
    public partial interface IWeatherModule : IModule {
    }
    """;

  [Fact(Skip = "as")]
  public void Generator_GeneratesEmptyModuleClass_WhenModuleDeclarationIsEmpty() {
    new SourceGeneratorTest<ModuleMumeiGenerator>(
        b => b
          .AddSource(VectorClassText, x => x.WithUsing<ModuleAttribute>())
      )
      .Run()
      .Should()
      .HaveGeneratedFile("IWeatherModule.g.cs")
      .WithGeneratedContent(ExpectedGeneratedClassText, x => x.WithUsing<IModule>());
  }
}