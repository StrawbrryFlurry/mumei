using Mumei.CodeGen.Qt.Output;

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

        RuntimeTypeSerializer.SerializeInto(writer, t, format);
    }
}