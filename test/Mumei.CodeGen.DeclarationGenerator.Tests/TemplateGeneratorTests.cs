using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.CodeGen.DeclarationGenerator.Tests;

public sealed class TemplateGeneratorTests {
    private static (GeneratorDriverRunResult Result, Compilation Output) Run(string source) {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && a.Location != "")
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Append(MetadataReference.CreateFromFile(typeof(ClassTemplate).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            "Test",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
        );

        var driver = CSharpGeneratorDriver.Create(new TemplateGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
        return (driver.GetRunResult(), output);
    }

    [Fact]
    public void ValidTemplate_GeneratesCompilingCode() {
        var (result, output) = Run(
            """
            using Mumei.CodeGen;
            namespace App;

            public partial record Greeter : ClassTemplate {
                public required string Greeting { get; init; }
                public string Greet(string name) => $"{Greeting}, {name}!";
            }
            """
        );

        Assert.Empty(result.Diagnostics);
        Assert.Empty(output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        var source = Assert.Single(result.GeneratedTrees).ToString();
        Assert.Contains(""".InterpolatedValue(Greeting).Text(", {name}!\";")""", source);
    }

    [Theory]
    [InlineData("public record Greeter : ClassTemplate;", "MUMEI101")]
    [InlineData("public partial class Outer { public partial record Greeter : ClassTemplate; }", "MUMEI102")]
    [InlineData("public partial record Greeter<T> : ClassTemplate;", "MUMEI103")]
    [InlineData(
        """
        public partial record Greeter : ClassTemplate {
            public required string Greeting { get; init; }
            public string Greet(string name) => Greeting.Replace("x", name);
        }
        """,
        "MUMEI104"
    )]
    [InlineData(
        """
        public partial record Greeter : ClassTemplate {
            [Generated] private static string Greet() => "";
        }
        """,
        "MUMEI105"
    )]
    [InlineData(
        """
        public partial record Greeter : InterceptorTemplate {
            public override TResult Intercept<TResult>(Call<TResult> call) => call.Proceed();
            public void Helper() { }
        }
        """,
        "MUMEI106"
    )]
    public void InvalidTemplate_ReportsDiagnostic(string declaration, string id) {
        var (result, _) = Run($"using Mumei.CodeGen;\nnamespace App;\n{declaration}");

        Assert.Contains(result.Diagnostics, d => d.Id == id);
    }
}
