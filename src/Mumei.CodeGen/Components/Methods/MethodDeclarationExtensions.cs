namespace Mumei.CodeGen.Components;

public static class MethodDeclarationExtensions {
    public static ISyntheticMethodBuilder<TSignature> DeclareMethodFrom<TSignature>(
        this ISyntheticClassBuilder<ISyntheticClass> classBuilder,
        Delegate implementationRef
    ) where TSignature : Delegate {
        return new SyntheticMethodBuilder<TSignature>(implementationRef.Method.Name, classBuilder.ΦCompilerApi);
    }
}