namespace Mumei.CodeGen.SyntaxWriters;

public class SyntaxTypeContext {
  private readonly HashSet<string> _usedNamespaces = new();
  internal IEnumerable<string> UsedNamespaces => _usedNamespaces;

  public void UseNamespace(string ns) {
    _usedNamespaces.Add(ns);
  }
}