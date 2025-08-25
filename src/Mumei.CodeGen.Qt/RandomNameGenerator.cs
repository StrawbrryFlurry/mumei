#pragma warning disable RS1035 // We need random (It's in the name)

namespace Mumei.CodeGen.Qt;

internal static class RandomNameGenerator {
    private static readonly Random Rng = new();

    private static ReadOnlySpan<char> Alphabet => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int ObfuscationPartLength = 12;
    private const int SpacerLength = 1;
    private const int MaxStackAllocLength = 128;

    public static string GenerateName(ReadOnlySpan<char> hint) {
        var totalLength = ObfuscationPartLength + hint.Length + SpacerLength;
        if (totalLength < MaxStackAllocLength) {
            Span<char> buffer = stackalloc char[totalLength];
            return GenerateNameCore(buffer, hint);
        }

        return GenerateNameCore(new char[totalLength], hint);
    }

    private static unsafe string GenerateNameCore(Span<char> buffer, ReadOnlySpan<char> hint) {
        for (var i = 0; i < ObfuscationPartLength; i++) {
            var idx = Rng.Next(0, Alphabet.Length);
            buffer[i] = Alphabet[idx];
        }

        buffer[ObfuscationPartLength - 1] = '_';
        hint.CopyTo(buffer[ObfuscationPartLength..]);

        // The slow path allocates on the heap, so this needs to be fixed.
        fixed (char* resultBufferPtr = buffer) {
            return new string(resultBufferPtr);
        }
    }
}