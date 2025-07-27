using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtAttribute(
    IQtType type,
    IQtCompileTimeValue[]? arguments = null,
    Dictionary<string, IQtCompileTimeValue>? namedArguments = null
) : IQtTemplateBindable {
    public static QtAttribute FromType<TAttribute>() where TAttribute : Attribute {
        return FromType(typeof(TAttribute));
    }

    public static QtAttribute FromType<TAttribute>(params IQtCompileTimeValue[]? arguments) where TAttribute : Attribute {
        return new QtAttribute(
            QtType.ForRuntimeType<TAttribute>(),
            arguments
        );
    }

    public static QtAttribute FromType<TAttribute>(Dictionary<string, IQtCompileTimeValue>? namedArguments) where TAttribute : Attribute {
        return FromType(
            typeof(TAttribute),
            null,
            namedArguments
        );
    }

    public static QtAttribute FromType(
        Type t,
        IQtCompileTimeValue[]? arguments = null,
        Dictionary<string, IQtCompileTimeValue>? namedArguments = null
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
    QtAttribute[] attributes
) : IQtTemplateBindable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter { }
}