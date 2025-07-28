using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public interface IQtType : IQtTemplateBindable;

public sealed class QtType {
    public static IQtType ForRuntimeType<T>() {
        return new QtRuntimeType(typeof(T));
    }

    public static IQtType ForExpression(in QtExpression expression) {
        return new QtExpressionType(expression);
    }

    public static IQtType ForRuntimeType(Type t) {
        return new QtRuntimeType(t);
    }

    public static IQtType ForRoslynType(ITypeSymbol t) {
        return new QtRoslynType(t);
    }

    public static IQtType ConstructRuntimeGenericType(
        Type t,
        params IQtType[] typeArguments
    ) {
        if (!t.IsGenericType) {
            throw new ArgumentException($"Type '{t.FullName}' is not a generic type.", nameof(t));
        }

        var type = new QtRuntimeType(t);
        return new CompositeQtType(type, typeArguments);
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
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (attributes.HasFlag(QtTypeAttribute.ByRef)) {
            writer.Write("ref ");
        }

        if (attributes.HasFlag(QtTypeAttribute.Readonly)) {
            writer.Write("readonly ");
        }

        if (attributes.HasFlag(QtTypeAttribute.Pointer)) {
            writer.Write("*");
        }

        RuntimeTypeSerializer.SerializeInto(ref writer, t, format);
    }

    public override string ToString() {
        return t.ToString();
    }
}

file sealed class CompositeQtType(
    IQtType type,
    IQtType[] typeArguments
) : IQtType {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (format == "t") {
            writer.Write("typeof(");
        }

        writer.WriteFormatted($"{type:g}<");
        for (var i = 0; i < typeArguments.Length; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            writer.Write(typeArguments[i]);
        }

        writer.Write(">");

        if (format == "t") {
            writer.Write(")");
        }
    }

    public override string ToString() {
        var sw = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
        WriteSyntax(ref sw);
        return sw.ToString();
    }
}

file sealed class QtRoslynType(
    ITypeSymbol typeSymbol
) : IQtType {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (format == "t") {
            writer.Write("typeof(");
        }

        writer.Write(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        if (format == "t") {
            writer.Write(")");
        }
    }

    public override string ToString() {
        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}

file sealed class QtExpressionType(
    QtExpression expression
) : IQtType {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(expression);
    }
}