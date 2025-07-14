using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Playground;

public enum AccessModifier {
    Public,
    PublicReadonly,
    PublicSealed,
    PublicStatic,
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
    Readonly
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
            AccessModifier.Readonly => "readonly",
            _ => throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null)
        };
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

    public void WriteSyntax<TSyntaxWriter>(in TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(modifier.AsCSharpString());
    }
}