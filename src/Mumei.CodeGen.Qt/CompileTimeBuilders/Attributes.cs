using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Playground;

public enum AccessModifier {
    Public,
    PublicReadonly,
    PublicSealed,
    PublicStatic,
    PublicAbstract,
    Private,
    PrivateReadonly,
    PrivateSealed,
    PrivateStatic,
    Protected,
    ProtectedSealed,
    ProtectedStatic,
    Internal,
    InternalSealed,
    InternalStatic,
    File,
    FileReadonly,
    FileSealed,
    FileStatic,
    Readonly
}

[Flags]
public enum ParameterAttributes {
    None = 0,
    This = 1 << 0,
    Ref = 1 << 5,
    Out = 1 << 6,
    In = 1 << 7,
    Params = 1 << 8,
    Readonly = 1 << 9
}

public static class AccessModifierExtensions {
    public static string AsCSharpString(this AccessModifier modifier) {
        return modifier switch {
            AccessModifier.Public => "public",
            AccessModifier.PublicSealed => "public sealed",
            AccessModifier.PublicReadonly => "public readonly",
            AccessModifier.PublicStatic => "public static",
            AccessModifier.Private => "private",
            AccessModifier.PrivateSealed => "private sealed",
            AccessModifier.PrivateReadonly => "private readonly",
            AccessModifier.PrivateStatic => "private static",
            AccessModifier.Protected => "protected",
            AccessModifier.ProtectedSealed => "protected sealed",
            AccessModifier.ProtectedStatic => "protected static",
            AccessModifier.Internal => "internal",
            AccessModifier.InternalSealed => "internal sealed",
            AccessModifier.InternalStatic => "internal static",
            AccessModifier.File => "file",
            AccessModifier.FileSealed => "file sealed",
            AccessModifier.FileStatic => "file static",
            AccessModifier.FileReadonly => "file readonly",
            AccessModifier.Readonly => "readonly",
            _ => throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null)
        };
    }

    public static bool IsAbstract(this AccessModifier modifier) {
        return modifier is AccessModifier.PublicAbstract;
    }

    public static AccessModifierRepresentable Represent(this AccessModifier modifier) {
        return new AccessModifierRepresentable(modifier);
    }
}

public readonly struct AccessModifierRepresentable(
    AccessModifier modifier
) : ISyntaxRepresentable {
    public static implicit operator AccessModifierRepresentable(AccessModifier modifier) {
        return new AccessModifierRepresentable(modifier);
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(modifier.AsCSharpString());
    }
}