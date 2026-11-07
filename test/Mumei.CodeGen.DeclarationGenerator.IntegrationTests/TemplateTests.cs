using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Testing;
using static Mumei.Roslyn.Testing.SourceGeneratorTest;

namespace Mumei.CodeGen.DeclarationGenerator.IntegrationTests;

public sealed class TemplateTests {
    private const string EntitySource =
        """
        namespace App.Models;

        [System.AttributeUsage(System.AttributeTargets.Class)]
        public sealed class EntityAttribute : System.Attribute;

        [Entity]
        public sealed class User;
        """;

    private const string MathSource =
        """
        namespace App;

        public static class Calculator {
            public static int Double(int value) => value * 2;
            public static void Reset() { }
        }

        public static class Program {
            public static int Run() {
                Calculator.Reset();
                return Calculator.Double(21);
            }
        }
        """;

    [Fact]
    public void ClassTemplate_GenericTemplate_EmitsMembersWithBoundTypes() {
        MumeiGeneratorTest(
            b => b.AddSource("User.cs", EntitySource),
            gen => gen.TypesWith("App.Models.EntityAttribute").Generate(type => new Repository { Entity = type })
        ).RunWithAssert(result => result.HasFileMatching("*UserRepository*.g.cs").WithPartialContent(
            $$"""
            namespace App.Models;

            public class UserRepository {
                private readonly global::System.Collections.Generic.Dictionary<int, global::App.Models.User> _items = new();

                public global::App.Models.User? Find(int id) => global::System.Collections.Generic.CollectionExtensions.GetValueOrDefault(_items, id);

                public void Add(int id, global::App.Models.User entity) {
                    _items[id] = entity;
                    global::System.Console.WriteLine($"Added to UserRepository");
                }
            }
            """
        ));
    }

    [Fact]
    public void ClassTemplate_GeneratedMethodAndInterfaces_AreImplementedThroughBuild() {
        const string source =
            """
            namespace App;

            [System.AttributeUsage(System.AttributeTargets.Class)]
            public sealed class RequestAttribute : System.Attribute;

            [Request] public sealed class Ping;
            [Request] public sealed class Pong;
            """;

        MumeiGeneratorTest(
            b => b.AddSource("Requests.cs", source),
            gen => gen.TypesWith("App.RequestAttribute")
                .Select(type => type.Type)
                .Collect()
                .Generate(requests => new Dispatcher { Requests = requests })
        ).RunWithAssert(result => result.HasFileMatching("*Dispatcher*.g.cs").WithPartialContent(
            $$"""
                public sealed class Dispatcher : global::System.IDisposable {
                    public string Describe(object request) => Dispatch(request) ?? "unknown";

                    public void Dispose() { }

                    private static string? Dispatch(object request) {
                        // Dispatching 2 requests
                        return request switch {
                            global::App.Ping => "Ping",
                            global::App.Pong => "Pong",
                            _ => null
                        };
                    }
                }
            """
        ));
    }

    [Fact]
    public void InterceptorTemplate_ProceedsAndInlinesInputs() {
        MumeiGeneratorTest(
            b => b.AddSource("Calculator.cs", MathSource),
            gen => gen.Calls("Double").Intercept(_ => new LogCall { Message = "Doubling" })
        ).RunWithAssert(result => result.HasFileMatching("*DoubleInterceptor*.g.cs").WithPartialContent(
            $$"""
                    public static int Intercept(int value) {
                        global::System.Console.WriteLine("Doubling");
                        var sw = global::System.Diagnostics.Stopwatch.StartNew();
                        try {
                            return global::App.Calculator.Double(value);
                        } finally {
                            global::System.Console.WriteLine($"Double took {sw.ElapsedMilliseconds}ms");
                        }
                    }
            """
        ));
    }

    [Fact]
    public void InterceptorTemplate_VoidTarget_ProceedsWithoutReturn() {
        MumeiGeneratorTest(
            b => b.AddSource("Calculator.cs", MathSource),
            gen => gen.Calls("Reset").Intercept(_ => new LogCall { Message = "Resetting" })
        ).RunWithAssert(result => result.HasFileMatching("*ResetInterceptor*.g.cs").WithPartialContent(
            $$"""
                        try {
                            global::App.Calculator.Reset();
                        } finally {
            """
        ));
    }

    [Fact]
    public void InterceptorTemplate_ProceedWithArguments_StagesArguments() {
        MumeiGeneratorTest(
            b => b.AddSource("Calculator.cs", MathSource),
            gen => gen.Calls("Double").Intercept(_ => new ClampArgument { Max = 10 })
        ).RunWithAssert(result => result.HasFileMatching("*DoubleInterceptor*.g.cs").WithPartialContent(
            $$"""
                    public static int Intercept(int value) {
                        var clamped = global::System.Math.Min(value, 10);
                        return global::App.Calculator.Double(clamped);
                    }
            """
        ));
    }

    [Fact]
    public void Templates_UnrelatedEdit_CachesOutput() {
        MumeiGeneratorTest(
            b => b.AddSource("User.cs", EntitySource).AddSource("Other.cs", "namespace App; class Other;"),
            gen => gen.TypesWith("App.Models.EntityAttribute").Generate(type => new Repository { Entity = type })
        )
            .RunWithAssert(_ => { })
            .UpdateCompilation(c => c.UpdateFile("Other.cs", "namespace App; class Other { void M() { } }"))
            .RunWithAssert(result => {
                Assert.NotEmpty(result.OutputReasons);
                Assert.All(result.OutputReasons, reason => Assert.Equal(IncrementalStepRunReason.Cached, reason));
            });
    }
}
