namespace Mumei.CodeGen.Components;

internal sealed class CompileTimeComponentUsedAtRuntimeException() : InvalidOperationException(
    "A compile-time only component was used at runtime."
);