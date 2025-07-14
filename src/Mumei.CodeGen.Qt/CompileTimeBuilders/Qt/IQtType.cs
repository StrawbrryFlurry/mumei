using System.Runtime.CompilerServices;
using Mumei.CodeGen.Qt.Output;
using Mumei.Roslyn.Reflection;

namespace Mumei.CodeGen.Qt.Qt;

public interface IQtType : IQtTemplateBindable;

public sealed class QtType {
    public static IQtType ForRuntimeType<T>() {
        return new QtRuntimeType(typeof(T));
    }

    public static IQtType ForRuntimeType(Type t) {
        return new QtRuntimeType(t);
    }
}

[Flags]
public enum QtTypeAttribute {
    None = 0,
    ByRef = 1 << 0,
    Readonly = 1 << 1,
    Pointer = 1 << 3
}

file sealed class QtRuntimeType(
    Type t,
    QtTypeAttribute attributes = QtTypeAttribute.None
) : IQtType {
    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (attributes.HasFlag(QtTypeAttribute.ByRef)) {
            writer.Write("ref ");
        }

        if (attributes.HasFlag(QtTypeAttribute.Readonly)) {
            writer.Write("readonly ");
        }

        if (attributes.HasFlag(QtTypeAttribute.Pointer)) {
            writer.Write("*");
        }

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
        if (TryWriteShortForm(type, writer)) {
            return;
        }

        if (format is "g" or "global") {
            writer.Write("global::");
        }

        if (type.Namespace is not null) {
            writer.Write(type.Namespace);
            writer.Write(".");
        }

        ReadOnlySpan<char> name = type.Name;
        if (type.IsGenericType) {
            name = name[..name.LastIndexOf('`')];
        }

        writer.Write(name);

        if (!type.IsGenericType) {
            return;
        }

        writer.Write("<");
        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            WriteSyntaxCore(genericArguments[i], writer, format);
        }

        writer.Write(">");
    }

    private static bool TryWriteShortForm<TSyntaxWriter>(Type type, TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        if (type.IsPrimitive) {
            writer.Write(type switch {
                not null when type == typeof(int) => "int",
                not null when type == typeof(uint) => "uint",
                not null when type == typeof(long) => "long",
                not null when type == typeof(ulong) => "ulong",
                not null when type == typeof(short) => "short",
                not null when type == typeof(ushort) => "ushort",
                not null when type == typeof(byte) => "byte",
                not null when type == typeof(sbyte) => "sbyte",
                not null when type == typeof(float) => "float",
                not null when type == typeof(double) => "double",
                not null when type == typeof(bool) => "bool",
                not null when type == typeof(char) => "char",
                _ => throw new NotSupportedException($"Unsupported primitive type: {type}")
            });
            return true;
        }

        if (type == typeof(string)) {
            writer.Write("string");
            return true;
        }

        return false;
    }
}