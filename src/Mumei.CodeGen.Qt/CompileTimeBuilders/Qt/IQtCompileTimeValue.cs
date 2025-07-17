using System.Runtime.CompilerServices;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public interface IQtCompileTimeValue : IQtTemplateBindable {
    public TActual As<TActual>();
}

public static class QtCompileTimeValue {
    public static IQtCompileTimeValue Null { get; } = QtNullCompileTimeValue.Instance;

    public static IQtCompileTimeValue ForLiteral<T>(T value) where T : notnull {
        return new QtLiteralCompileTimeValue<T>(value);
    }
}

internal sealed class QtLiteralCompileTimeValue<T> : IQtCompileTimeValue where T : notnull {
    private readonly T _value;

    public QtLiteralCompileTimeValue(T value) {
        _value = value;
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteLiteral(_value);
    }

    public TActual As<TActual>() {
        if (_value is TActual actual) {
            return actual;
        }

        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}

internal sealed class QtNullCompileTimeValue : IQtCompileTimeValue {
    public static QtNullCompileTimeValue Instance { get; } = new();

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write("null");
    }

    public TActual As<TActual>() {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}