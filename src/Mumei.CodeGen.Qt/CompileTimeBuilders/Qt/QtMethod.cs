using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

public readonly struct QtMethod<TReturnType> : IQtInvokable<TReturnType> {
    private readonly QtDeclarationPtr<QtMethodRenderFragment> _declarationPtr;

    internal QtMethodRenderFragment Method { get; }

    internal QtMethod(
        string name,
        AccessModifier modifiers,
        IQtType returnType,
        in QtTypeParameterList typeParameters,
        in QtParameterList parameters,
        in CodeBlockFragment codeBlock,
        in QtAttributeList attributes,
        in QtDeclarationPtr<QtMethodRenderFragment> declarationPtr
    ) {
        _declarationPtr = declarationPtr;
        Method = new QtMethodRenderFragment(
            modifiers,
            name,
            returnType,
            typeParameters,
            parameters,
            codeBlock,
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

internal readonly struct QtMethodRenderFragment(
    AccessModifier modifiers,
    string name,
    IQtType returnType,
    QtTypeParameterList typeParameters,
    QtParameterList parameters,
    CodeBlockFragment codeBlock,
    QtAttributeList attributes
) : IQtTemplateBindable, IRenderFragment, IDebugRenderNodeFormattable {
    public string Name => name;

    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        if (!attributes.IsEmpty) {
            writer.Write(attributes);
            writer.WriteLine();
        }

        writer.WriteFormatted($"{modifiers} {returnType:g} {name}{typeParameters}({parameters})");

        // if (!typeParameters.Constraints.IsEmpty) {
        //     writer.WriteFormatted($" {typeParameters.Constraints}");
        // }

        if (modifiers.IsAbstract()) {
            writer.Write(";");
            return;
        }

        writer.Write(" ");
        writer.WriteLine("{");
        writer.Indent();
        // writer.WriteBlock(codeBlock);
        writer.Dedent();
        writer.Write("}");
    }

    public void Render(IRenderTreeBuilder tree) {
        if (!attributes.IsEmpty) {
            tree.Bind(attributes);
            tree.NewLine();
        }

        tree.Interpolate($"{modifiers.AsCSharpString()} {returnType.Expression} {name}{typeParameters}({parameters})");

        if (!typeParameters.Constraints.IsEmpty) {
            tree.Interpolate($" {typeParameters.Constraints}");
        }

        if (modifiers.IsAbstract()) {
            tree.Text(";");
            return;
        }

        tree.Text(" ");
        tree.StartCodeBlock();
        tree.Node(codeBlock);
        tree.NewLine();
        tree.EndCodeBlock();
    }

    public string DescribeDebugNode() {
        return $"QtMethod {{ Name = {name} }}";
    }
}

public readonly struct QtMethodStub {
    public required string Name { get; init; }
    public required bool IsThisCall { get; init; }
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

internal readonly struct QtInvocation : IQtTemplateBindable {
    public required QtExpression Target { get; init; }
    public required QtMethodStub Method { get; init; }
    public required QtTypeArgumentList TypeArguments { get; init; }
    public required QtArgumentList Arguments { get; init; }

    public static QtInvocation Static(IQtType target, QtMethodStub method, QtTypeArgumentList typeArguments, QtArgumentList arguments) {
        var targetExpression = QtExpression.ForBindable(target);
        return new QtInvocation {
            Target = targetExpression,
            Method = method,
            TypeArguments = typeArguments,
            Arguments = arguments
        };
    }

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