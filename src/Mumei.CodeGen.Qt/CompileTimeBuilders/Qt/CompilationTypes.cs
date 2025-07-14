using Mumei.CodeGen.Playground;

namespace Mumei.CodeGen.Qt.Qt;

public sealed class Unit;

public sealed class CompileTimeUnknown;

public sealed class BooleanLike {
    public static explicit operator bool(BooleanLike compileTimeUnknown) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }
}