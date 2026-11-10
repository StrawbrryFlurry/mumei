using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Mumei.CodeGen;

internal static class RuntimeTypeSerializer {
    internal static bool TryGetKeywordType(Type type, [NotNullWhen(true)] out string? keyword) {
        keyword = type switch {
            _ when type == typeof(int) => "int",
            _ when type == typeof(uint) => "uint",
            _ when type == typeof(long) => "long",
            _ when type == typeof(ulong) => "ulong",
            _ when type == typeof(short) => "short",
            _ when type == typeof(ushort) => "ushort",
            _ when type == typeof(byte) => "byte",
            _ when type == typeof(sbyte) => "sbyte",
            _ when type == typeof(float) => "float",
            _ when type == typeof(double) => "double",
            _ when type == typeof(bool) => "bool",
            _ when type == typeof(char) => "char",
            _ when type == typeof(decimal) => "decimal",
            _ when type == typeof(nint) => "nint",
            _ when type == typeof(nuint) => "nuint",
            _ when type == typeof(string) => "string",
            _ when type == typeof(object) => "object",
            _ when type == typeof(void) => "void",
            _ => null
        };

        return keyword is not null;
    }

    /// <summary>
    /// Returns the globally qualified C# name of <paramref name="type"/>,
    /// e.g. <c>global::System.Collections.Generic.Dictionary&lt;string, global::Outer.Inner&gt;</c>.
    /// </summary>
    internal static string GetTypeFullName(Type type, bool global) {
        var sb = new StringBuilder();
        Append(sb, type, global);
        return sb.ToString();
    }

    /// <summary>
    /// Returns the metadata name of the type's definition, e.g. <c>System.Collections.Generic.List`1</c> or <c>Outer+Inner</c>.
    /// </summary>
    internal static string GetMetadataName(Type type) {
        if (type.IsConstructedGenericType) {
            type = type.GetGenericTypeDefinition();
        }

        return type.FullName ?? type.Name;
    }

    private static void Append(StringBuilder sb, Type type, bool global) {
        if (TryGetKeywordType(type, out var keyword)) {
            sb.Append(keyword);
            return;
        }

        if (type.IsGenericParameter) {
            sb.Append(type.Name);
            return;
        }

        if (type.IsArray) {
            Append(sb, type.GetElementType()!, global);
            sb.Append('[').Append(',', type.GetArrayRank() - 1).Append(']');
            return;
        }

        if (type.IsPointer) {
            Append(sb, type.GetElementType()!, global);
            sb.Append('*');
            return;
        }

        if (type.IsByRef) {
            Append(sb, type.GetElementType()!, global);
            return;
        }

        if (Nullable.GetUnderlyingType(type) is { } nullableOf) {
            Append(sb, nullableOf, global);
            sb.Append('?');
            return;
        }

        if (global) {
            sb.Append("global::");
        }

        if (!string.IsNullOrEmpty(type.Namespace)) {
            sb.Append(type.Namespace).Append('.');
        }

        // Nested types share the generic arguments of their containing types:
        // Outer<T>.Inner<U> is represented as Outer`1+Inner`1 with the arguments [T, U].
        var chain = new List<Type>();
        for (var current = type; current is not null; current = current.IsNested ? current.DeclaringType : null) {
            chain.Add(current);
        }

        chain.Reverse();
        var genericArguments = type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;
        var consumedArguments = 0;

        for (var i = 0; i < chain.Count; i++) {
            if (i > 0) {
                sb.Append('.');
            }

            var name = chain[i].Name;
            var tickIndex = name.IndexOf('`');
            sb.Append(tickIndex >= 0 ? name.AsSpan(0, tickIndex) : name);

            var totalArity = chain[i].IsGenericType ? chain[i].GetGenericArguments().Length : 0;
            var ownArity = totalArity - consumedArguments;
            if (ownArity <= 0) {
                continue;
            }

            sb.Append('<');
            for (var a = 0; a < ownArity; a++) {
                if (a > 0) {
                    sb.Append(", ");
                }

                var argument = genericArguments[consumedArguments + a];
                if (type.IsGenericTypeDefinition) {
                    sb.Append(argument.Name);
                }
                else {
                    Append(sb, argument, global);
                }
            }

            sb.Append('>');
            consumedArguments = totalArity;
        }
    }
}