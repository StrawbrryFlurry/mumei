﻿using Mumei.Roslyn.SourceGeneration;
using Mumei.Roslyn.Testing;

namespace Mumei.Roslyn.Tests;

public sealed class TestSourceGeneratorTests {
  [Fact]
  public async Task TestSourceGeneratorTest() {
    var source = """
                 namespace Mumei.Test;

                 public interface IWeatherModule {
                   public string GetWeather();
                 }
                 """;
  }

  [Fact(Skip = "Example")]
  public void Test1() {
    var otherSource = """
                      using Mumei.CodeGen.SyntaxNodes;

                      public static class OtherClass {
                        public static void Act(ClassSyntaxBuilder builder) {
                          builder.AddField<string>("Baz");
                        }
                      }
                      """;

    var source = """
                 using Mumei.CodeGen.SyntaxNodes;

                 public class TestClass {
                   public void TestMethod() {
                     var builder = new ClassSyntaxBuilder<Testt>();
                     builder.AddField<string>("Foo");
                     builder.AddField<string>("Bar");
                     OtherClass.Act(builder);
                     builder = new ClassSyntaxBuilder<TesttT>();
                     builder.AddField<string>("Foo");
                   }
                 }
                 """;

    var expected = """
                   public class Testt {
                   }
                   """;
  }
}