using Mumei.CodeGen.Qt.Output;
using Mumei.Roslyn.Reflection;

namespace Mumei.CodeGen.Qt.Qt;

public interface IQtType : IQtTemplateBindable, IQtComponent;

public sealed class QtType {
    public static IQtType ForRuntimeType<T>() {
        return new QtRuntimeType(typeof(T));
    }

    public static IQtType ForRuntimeType(Type t) {
        return new QtRuntimeType(t);
    }
}

file sealed class QtRuntimeType(
    Type t
) : IQtType {
    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        WriteSyntaxCore(
            t,
            in writer,
            format
        );
    }

    public static void WriteSyntaxCore<TSyntaxWriter>(
        Type type,
        in TSyntaxWriter writer,
        string? format = null
    ) where TSyntaxWriter : ISyntaxWriter {
        if (format is "g" or "global") {
            writer.Write("global::");
        }

        if (type.Namespace is not null) {
            writer.Write(type.Namespace);
            writer.Write(".");
        }

        writer.Write(type.Name);

        if (!type.IsGenericType) {
            return;
        }

        writer.Write("<");
        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            writer.Write(genericArguments[i].Name);
        }
    }
}