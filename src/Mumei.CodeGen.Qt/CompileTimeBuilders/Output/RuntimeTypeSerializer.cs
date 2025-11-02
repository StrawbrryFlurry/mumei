using System.Diagnostics.CodeAnalysis;

namespace Mumei.CodeGen.Qt.Output;

internal static class RuntimeTypeSerializer {
    public static void SerializeInto<TSyntaxWriter>(
        ref TSyntaxWriter writer,
        Type type,
        string? format = null
    ) where TSyntaxWriter : ISyntaxWriter {
        if (format == "t") {
            writer.WriteFormatted($"typeof(");
        }

        if (TryWriteShortForm(type, ref writer)) {
            goto WriteEndOfType;
        }

        writer.Write("global::");

        if (type.Namespace is not null) {
            writer.Write(type.Namespace);
            writer.Write(".");
        }

        ReadOnlySpan<char> name = type.Name;
        if (type.IsGenericType) {
            name = name[..name.LastIndexOf('`')];
        }

        writer.Write(name);

        if (!type.IsConstructedGenericType) {
            goto WriteEndOfType;
        }

        writer.Write("<");
        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            SerializeInto(ref writer, genericArguments[i], format);
        }

        writer.Write(">");

        WriteEndOfType:
        if (format == "t") {
            writer.Write(")");
        }
    }

    public static void RenderInto(IRenderTreeBuilder tree, Type type) {
        if (TryWriteShortForm(type, tree)) {
            return;
        }

        tree.Text("global::");

        if (type.Namespace is not null) {
            tree.Text(type.Namespace);
            tree.Text(".");
        }

        ReadOnlySpan<char> name = type.Name;
        if (type.IsGenericType) {
            name = name[..name.LastIndexOf('`')];
        }

        tree.Text(name);

        if (!type.IsConstructedGenericType) {
            return;
        }

        tree.Text("<");
        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++) {
            if (i > 0) {
                tree.Text(", ");
            }

            RenderInto(tree, genericArguments[i]);
        }

        tree.Text(">");
    }

    private static bool TryWriteShortForm(Type type, IRenderTreeBuilder tree) {
        if (TryGetKeywordType(type, out var keywordType)) {
            tree.Text(keywordType);
            return true;
        }

        return false;
    }
    private static bool TryGetKeywordType(Type type, [NotNullWhen(true)] out string? keyword) {
        if (type.IsPrimitive) {
            keyword = type switch {
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
            };
            return true;
        }

        if (type == typeof(string)) {
            keyword = "string";
            return true;
        }

        keyword = null!;
        return false;
    }

    private static bool TryWriteShortForm<TSyntaxWriter>(Type type, ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
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

    internal static string GetTypeFullName(Type type) {
        if (TryGetKeywordType(type, out var keywordType)) {
            return keywordType;
        }

        var writer = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
        SerializeInto(ref writer, type);
        return writer.ToString();
    }
}