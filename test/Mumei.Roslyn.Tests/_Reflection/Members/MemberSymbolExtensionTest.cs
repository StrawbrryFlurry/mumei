using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing;
using Mumei.Roslyn.Testing.Comp;

namespace Mumei.Roslyn.Tests.Reflection.Members;

public abstract class MemberSymbolExtensionTest<TSymbol, TMemberInfo>
  where TSymbol : ISymbol
  where TMemberInfo : MemberInfo {
  private const string TestTypeSource = """
                                        namespace Test;

                                        public class TestDeclaringType { }
                                        """;

  private Compilation? _compilation;

  protected virtual string DeclaringTypeFullyQualifiedName { get; } = "Test.TestType";

  protected Compilation Compilation => _compilation ??= GetMemberTestCompilation();

  protected abstract TestCompilationBuilder CompilationBuilder { get; }

  private Compilation GetMemberTestCompilation() {
    return CompilationBuilder.AddSource(TestTypeSource).Build();
  }

  /// We use this additional class as the "declaring type"
  /// because using the actual declaring type symbol would
  /// already create the field info instance when scanning
  /// trough the type members. The tests would therefore
  /// only get the cached instance, which might break a
  /// test before the tested action on the member is even taken.
  protected ITypeSymbol GetEmptyDeclaringTypeSymbol() {
    return Compilation.GetTypeSymbol("Test.TestDeclaringType");
  }

  /// <inheritdoc cref="GetEmptyDeclaringTypeSymbol" />
  protected Type GetEmptyDeclaringType() {
    return GetEmptyDeclaringTypeSymbol().ToType();
  }

  #region Utilities

  protected abstract TMemberInfo CreateMemberInfo(TSymbol symbol, Type declaringType);

  protected TMemberInfo GetMemberInfoWithoutCreatingType(string memberName, out TSymbol memberSymbol) {
    memberSymbol = GetMemberSymbol(memberName);
    return CreateMemberInfo(memberSymbol, GetEmptyDeclaringType());
  }

  protected TMemberInfo GetMemberInfo(string memberName, out TSymbol memberSymbol) {
    memberSymbol = GetMemberSymbol(memberName);
    var declaringType = Compilation.GetTypeSymbol(DeclaringTypeFullyQualifiedName).ToType();
    return CreateMemberInfo(memberSymbol, declaringType);
  }

  protected TSymbol GetMemberSymbol(string name) {
    return Compilation.GetTypeMemberSymbol<TSymbol>(DeclaringTypeFullyQualifiedName, name);
  }

  #endregion
}