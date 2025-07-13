namespace Mumei.CodeGen.Playground;

public sealed class CompileTimeComponentUsedAtRuntimeException() : InvalidOperationException(
    "A compile-time method or template was used at runtime. " +
    "Compile-time components cannot be invoked and only exist for code generation purposes."
);