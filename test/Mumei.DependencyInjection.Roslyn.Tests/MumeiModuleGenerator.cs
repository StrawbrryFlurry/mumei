using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.Roslyn.Testing;
using Xunit;

namespace Mumei.DependencyInjection.Roslyn.Tests;

public sealed class SampleIncrementalSourceGeneratorTests {
  private string VectorClassText =
    $$"""
      using {{typeof(ModuleAttribute).Namespace}};

      [Module]
      public partial interface IWeatherModule {
      }
      """;

  private const string ExpectedGeneratedClassText =
    """
    using System;
    using System.Collections.Generic;

    namespace TestNamespace;

    partial class Vector3
    {
        public IEnumerable<string> Report()
        {
            yield return $""X:{this.X}"";
            yield return $""Y:{this.Y}"";
            yield return $""Z:{this.Z}"";
        }
    }
    """;

  [Fact]
  public void GenerateReportMethod() {
    new SourceGeneratorTest<ModuleMumeiGenerator>(
        b => b
          .AddSource(VectorClassText)
          .AddTypeReference<ModuleAttribute>()
      )
      .Run()
      .Should()
      .HaveGeneratedFile("Vector3.g.cs")
      .WithGeneratedContent(ExpectedGeneratedClassText);
  }
}