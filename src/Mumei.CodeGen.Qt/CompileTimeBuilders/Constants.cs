namespace Mumei.CodeGen.Playground;

internal static class IdentifierConstants {
    public const string InternalMarker = "λ";

    public static string Obfuscate(this string identifier) {
        return $"{InternalMarker}{identifier}";
    }
}

public static class Arg {
    public sealed class TThis { };

    public sealed class TReturn { };

    public sealed class ProxyThis { };

    public sealed class T1;

    public sealed class T2;

    public sealed class T3;

    public sealed class T4;
}