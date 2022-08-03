using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen;

[Generator]
public class ModuleGenerator : ISourceGenerator {
  public void Initialize(GeneratorInitializationContext context) {
    throw new NotImplementedException();
  }

  public void Execute(GeneratorExecutionContext context) {
    // var diagnostic = Diagnostic.Create("", "", "", "");
    // context.ReportDiagnostic(diagnostic);
  }
}