using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Roslyn.Components;

public static class AccessibilityExtensions {
    extension(Accessibility accessibility) {
        public AccessModifierList ToAccessModifiers() {
            return accessibility switch {
                Accessibility.Public => AccessModifier.Public,
                Accessibility.Private => AccessModifier.Private,
                Accessibility.Protected => AccessModifier.Protected,
                Accessibility.Internal => AccessModifier.Internal,
                Accessibility.ProtectedAndInternal => AccessModifier.Protected + AccessModifier.Internal,
                Accessibility.ProtectedOrInternal => AccessModifier.Protected + AccessModifier.Internal,
                _ => throw new ArgumentOutOfRangeException(nameof(accessibility), $"Unsupported accessibility: {accessibility}")
            };
        }
    }
}