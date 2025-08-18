using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Qt;

/// <summary>
/// Describes a piece of generated code that can be later bound
/// into a template, class or other QT Component.
///
/// <code>QtFactory.Fragment(() => {
///
/// })</code>
/// </summary>
public readonly struct QtFragment : IQtTemplateBindable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}

/// <summary>
/// Represents an arbitrary piece of code that can be bound into another template, namespace, or file.
/// </summary>
public readonly struct QtTemplate : IQtTemplateBindable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}