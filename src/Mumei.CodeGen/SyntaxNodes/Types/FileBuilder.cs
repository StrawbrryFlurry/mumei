using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public sealed class FileBuilder {
  public FileBuilder(string name) { }

  public ClassSyntaxBuilder AddClassDeclaration(string name, SyntaxVisibility classVisibility) {
    var classBuilder = new ClassSyntaxBuilder(name, classVisibility);
    return classBuilder;
  }
}