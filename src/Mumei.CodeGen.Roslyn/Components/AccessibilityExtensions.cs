using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    extension(SyntaxTokenList modifiers) {
        public AccessModifierList ToAccessModifiers() {
            var accessModifiers = new AccessModifierList();

            foreach (var modifier in modifiers) {
                accessModifiers += modifier.Kind() switch {
                    SyntaxKind.PublicKeyword => AccessModifier.Public,
                    SyntaxKind.PrivateKeyword => AccessModifier.Private,
                    SyntaxKind.AbstractKeyword => AccessModifier.Abstract,
                    SyntaxKind.ProtectedKeyword => AccessModifier.Protected,
                    SyntaxKind.InternalKeyword => AccessModifier.Internal,
                    SyntaxKind.FileKeyword => AccessModifier.File,
                    SyntaxKind.SealedKeyword => AccessModifier.Sealed,
                    SyntaxKind.ReadOnlyKeyword => AccessModifier.Readonly,
                    SyntaxKind.StaticKeyword => AccessModifier.Static,
                    SyntaxKind.PartialKeyword => AccessModifier.Partial,
                    SyntaxKind.VirtualKeyword => AccessModifier.Virtual,
                    SyntaxKind.OverrideKeyword => AccessModifier.Override,
                    SyntaxKind.AsyncKeyword => AccessModifier.Async,
                    _ => default
                };
            }

            return accessModifiers;
        }
    }
}