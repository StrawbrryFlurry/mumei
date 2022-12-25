using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing;

namespace Mumei.Roslyn.Tests.Reflection.Members;

public abstract class MemberSymbolExtensionTest {
  private const string TestTypeSource = """
  namespace Test;

  public class TestDeclaringType { }
  """;

  private Compilation? _compilation;

  protected Compilation Compilation => _compilation ??= GetMemberTestCompilation();

  protected abstract TestCompilationBuilder CompilationBuilder { get; }

  private Compilation GetMemberTestCompilation() {
    return CompilationBuilder.AddSourceText(TestTypeSource).Build();
  }

  /// We use this additional class as the "declaring type"
  /// because using the actual declaring type symbol would
  /// already create the field info instance when scanning
  /// trough the type members. The tests would therefore
  /// only get the cached instance, which might break a
  /// test before the tested action on the member is even taken.
  protected ITypeSymbol GetDeclaringTypeSymbol() {
    return Compilation.GetTypeSymbol("Test.TestDeclaringType");
  }

  /// <inheritdoc cref="GetDeclaringTypeSymbol" />
  protected Type GetDeclaringType() {
    return GetDeclaringTypeSymbol().ToType();
  }
}