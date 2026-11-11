using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mumei.CodeGen.Roslyn.Testing;

public static class SourceFile {
    public static DeclarationSourceComponent For<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDeclaration
    >() {
        throw new NotSupportedException(
            """
            This component is supposed to be intercepted by the SourceReferenceGenerator and cannot be called directly.
            """
        );
    }
}

public sealed record DeclarationSourceComponent(
    string Name,
    string Declaration,
    ICompilationSourceComponent[] ReferencedComponents
) : ICompilationSourceComponent {
    public void Register(TestCompilationBuilder builder) {
        builder.AddSource(Name, Declaration);
        foreach (var reference in ReferencedComponents) {
            reference.Register(builder);
        }
    }
}

public sealed record AssemblyCompilationSourceComponent(
    Assembly referencedAssembly
) : ICompilationSourceComponent {
    public void Register(TestCompilationBuilder builder) {
        builder.AddAssemblyReference(referencedAssembly);
    }
}

public interface ICompilationSourceComponent {
    public void Register(TestCompilationBuilder builder);
}