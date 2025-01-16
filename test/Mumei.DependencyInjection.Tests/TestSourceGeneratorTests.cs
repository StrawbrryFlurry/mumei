using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Providers.Registration;
using Mumei.DependencyInjection.Roslyn;
using Mumei.Roslyn.Testing;
using SourceCodeFactory;

namespace Mumei.DependencyInjection.Tests;

public sealed class TestSourceGeneratorTests {
    public sealed class SomeImpl : ISomeAbstraction { }

    public interface ISomeAbstraction { }

    [Module]
    public partial interface IWeatherModule {
        [Scoped<SomeImpl>]
        public ISomeAbstraction SomeAbstraction { get; }
    }

    [Fact]
    public async Task TestSourceGeneratorTest() {
        var weatherModule = SourceCode.Of<IWeatherModule>();

        static void AddReferences(TestCompilationBuilder builder, ITypeRef reference) {
            if (reference is SourceCodeTypeRef sctr) {
                builder.AddSource(sctr.SourceCode);

                foreach (var innerRefs in sctr.References) {
                    AddReferences(builder, innerRefs);
                }
            }
            else if (reference is AssemblyTypeRef atr) {
                builder.AddAssemblyReference(atr.AssemblyName);
            }
        }

        new SourceGeneratorTest<ModuleMumeiGenerator>(b => AddReferences(b, weatherModule))
            .Run()
            .Should()
            .HaveGeneratedFile("WeatherModule.ModuleImpl.g.cs")
            .WithContent($$"""
                           public sealed class λIWeatherModule : global::IWeatherModule {

                           }
                           """);
    }
}