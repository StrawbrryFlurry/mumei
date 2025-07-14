using System.Reflection;

namespace Mumei.CodeGen.Qt.Tests.Setup;

internal static class DynamicCompilationAssemblyAssertions {
    public static void PassesAssemblyAction(
        this SourceGeneratorTestResult runResult,
        Action<Assembly> assemblyAction
    ) {
        var inMemoryAssemblyStream = new MemoryStream();
        runResult.Compilation.Emit(inMemoryAssemblyStream);
        var assembly = Assembly.Load(inMemoryAssemblyStream.ToArray());

        assemblyAction(assembly);
    }
}