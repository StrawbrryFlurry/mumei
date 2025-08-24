using Mumei.CodeGen.Qt.Containers;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtAttribute(
    IQtType type,
    QtExpression[]? arguments = null,
    Dictionary<string, QtExpression>? namedArguments = null
) : IQtTemplateBindable {
    public static QtAttribute FromType<TAttribute>() where TAttribute : Attribute {
        return FromType(typeof(TAttribute));
    }

    public static QtAttribute FromType(IQtType type, QtExpression[] arguments) {
        return new QtAttribute(type, arguments);
    }

    public static QtAttribute FromType<TAttribute>(params QtExpression[]? arguments) where TAttribute : Attribute {
        return new QtAttribute(
            QtType.ForRuntimeType<TAttribute>(),
            arguments
        );
    }

    public static QtAttribute FromType<TAttribute>(Dictionary<string, QtExpression>? namedArguments) where TAttribute : Attribute {
        return FromType(
            typeof(TAttribute),
            null,
            namedArguments
        );
    }

    public static QtAttribute FromType(
        Type t,
        QtExpression[]? arguments = null,
        Dictionary<string, QtExpression>? namedArguments = null
    ) {
        return new QtAttribute(QtType.ForRuntimeType(t), arguments, namedArguments);
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(type);
        var hasArguments = arguments is not null && arguments.Length > 0 ||
                           namedArguments is not null && namedArguments.Count > 0;

        if (!hasArguments) {
            return;
        }

        writer.Write("(");
        if (arguments is not null) {
            writer.Write(arguments.AsMemory().RepresentAsSeparatedList(x => x));
        }

        if (namedArguments is not null) {
            var first = true;
            foreach (var arg in namedArguments) {
                if (!first) {
                    writer.Write(", ");
                }

                first = false;

                writer.Write(arg.Key);
                writer.Write(" = ");
                writer.Write(arg.Value);
            }
        }

        writer.Write(")");
    }
}

public readonly struct QtAttributeList(
    QtCollection<QtAttribute> attributes
) : IQtTemplateBindable, IQtMemoryAccessor<QtAttribute> {
    public int Length => attributes.Count;
    public bool IsEmpty => attributes.Count == 0;
    public static QtAttributeList Empty { get; } = new([]);

    public Memory<QtAttribute> Memory => attributes.Memory;

    public static QtAttributeList With(params QtCollection<QtAttribute> attributes) {
        return new QtAttributeList(attributes);
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (attributes.Count == 0) {
            return;
        }

        writer.Write("[");
        writer.Write(Memory.RepresentAsSeparatedList(x => x));
        writer.Write("]");
    }
}