using System.Diagnostics.CodeAnalysis;
using Mumei.CodeGen.Rendering;
using Mumei.Common.Internal;

namespace Mumei.CodeGen;

internal static class RuntimeTypeSerializer {
    public static void RenderInto(IRenderTreeBuilder tree, Type type) {
        if (TryGetKeywordType(type, out var keywordType)) {
            tree.Text(keywordType);
            return;
        }

        var buffer = new ArrayBuilder<char>(stackalloc char[ArrayBuilder.InitSize]);
        GetTypeFullName(type, ref buffer);
        tree.Text(buffer.Elements);
        buffer.Dispose();
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

    internal static string GetTypeFullName(Type type) {
        // Do this here to not re-allocate keyword strings
        if (TryGetKeywordType(type, out var keywordType)) {
            return keywordType;
        }

        var buffer = new ArrayBuilder<char>(stackalloc char[ArrayBuilder.InitSize]);
        GetTypeFullName(type, ref buffer);
        return buffer.ToStringAndFree();
    }

    internal static void GetTypeFullName(Type type, ref ArrayBuilder<char> buffer) {
        buffer.AddRange("global::");

        if (type.Namespace is not null) {
            buffer.AddRange(type.Namespace);
            buffer.Add('.');
        }

        ReadOnlySpan<char> name = type.Name;
        if (type.IsGenericType) {
            name = name[..name.LastIndexOf('`')];
        }

        buffer.AddRange(name);

        if (!type.IsConstructedGenericType) {
            return;
        }

        buffer.Add('<');
        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++) {
            if (i > 0) {
                buffer.AddRange(", ");
            }

            GetTypeFullName(genericArguments[i], ref buffer);
        }

        buffer.Add('>');
    }
}