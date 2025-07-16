namespace Mumei.CodeGen.Qt.Output;

internal static class RuntimeTypeSerializer {
    public static void SerializeInto<TSyntaxWriter>(
        ref TSyntaxWriter writer,
        Type type,
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

            SerializeInto(ref writer, genericArguments[i], format);
        }

        writer.Write(">");
    }

    private static bool TryWriteShortForm<TSyntaxWriter>(Type type, in TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
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