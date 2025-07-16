using System.Runtime.CompilerServices;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtMethod<TReturnType> : IQtInvokable<TReturnType> {
    private readonly QtDeclarationPtr<QtMethodCore> _declarationPtr;

    internal QtMethodCore Method { get; }

    internal QtMethod(
        string name,
        AccessModifier modifiers,
        IQtType returnType,
        QtTypeParameterList typeParameters,
        QtParameterList parameters,
        IQtMethodRepresentation representation,
        QtAttributeList attributes,
        QtDeclarationPtr<QtMethodCore> declarationPtr
    ) {
        _declarationPtr = declarationPtr;
        Method = new QtMethodCore(
            modifiers,
            name,
            returnType,
            typeParameters,
            parameters,
            representation,
            attributes
        );
    }

    public TReturnType Invoke(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TDynamicReturnType DynamicInvoke<TDynamicReturnType>(IQtThis target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        Method.WriteSyntax(ref writer, format);
    }
}

internal readonly struct QtMethodCore(
    AccessModifier modifiers,
    string name,
    IQtType returnType,
    QtTypeParameterList typeParameters,
    QtParameterList parameters,
    IQtMethodRepresentation representation,
    QtAttributeList attributes
) : IQtTemplateBindable {
    public string Name => name;

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.WriteFormatted($"{modifiers} {returnType:g} {name}{typeParameters}({parameters})");
        if (!typeParameters.Constraints.IsEmpty) {
            writer.WriteFormatted($" {typeParameters.Constraints}");
        }

        if (modifiers.IsAbstract()) {
            writer.Write(";");
            return;
        }

        representation.WriteSyntax(ref writer);
    }
}

public readonly struct QtMethodStub {
    public required string Name { get; init; }
    public required bool IsThisCall { get; init; }
}

internal interface IQtMethodRepresentation : ISyntaxRepresentable;

internal sealed class StaticQtMethodRepresentation(
    string s
) : IQtMethodRepresentation {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(s);
    }
}

public delegate QtMethodBuilder.Configured ConfigureQtMethod(QtMethodBuilder.StartDecl builder);

public static class QtMethodBuilder {
    public ref struct StartDecl {
        public ReturnsDecl Modifiers(AccessModifier accessModifier) {
            return new ReturnsDecl { };
        }
    }

    public ref struct ReturnsDecl {
        public ParametersDecl Returns(string name) {
            return new ParametersDecl { };
        }
    }

    public ref struct ParametersDecl {
        public Configured Takes(string name) {
            return new Configured { };
        }
    }

    public ref struct Configured { }
}

internal interface IQtInvocationTarget : IQtTemplateBindable { }

internal sealed class QtThisInvocationTarget : IQtInvocationTarget {
    public static readonly QtThisInvocationTarget Instance = new();
    private QtThisInvocationTarget() { }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write("this");
    }
}

internal sealed class QtInstanceInvocationTarget(
    string instanceIdentifier
) : IQtInvocationTarget {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(instanceIdentifier);
    }
}

internal sealed class QtStaticTypeInvocationTarget(
    IQtType target
) : IQtInvocationTarget {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(target, "g");
    }
}

internal readonly struct QtInvocation : IQtTemplateBindable {
    public required IQtInvocationTarget Target { get; init; }
    public required QtMethodStub Method { get; init; }
    public required QtTypeArgumentList TypeArguments { get; init; }
    public required QtArgumentList Arguments { get; init; }

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(Target);
        writer.Write(".");
        writer.Write(Method.Name);
        if (!TypeArguments.IsEmpty) {
            writer.WriteFormatted($"<{TypeArguments}>");
        }

        writer.Write("(");
        writer.Write(Arguments);
        writer.Write(")");
    }
}