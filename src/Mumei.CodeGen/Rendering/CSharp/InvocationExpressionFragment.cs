namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct InvocationExpressionFragment : IRenderFragment {
    public ExpressionFragment? Target { get; }
    public ExpressionFragment Method { get; }
    public ImmutableArray<ExpressionFragment> Arguments { get; }

    public InvocationExpressionFragment(
        ExpressionFragment? target,
        ExpressionFragment method,
        ImmutableArray<ExpressionFragment> arguments
    ) {
        Method = method;
        Arguments = arguments;
        Target = target;
    }

    public InvocationExpressionFragment WithTarget(ExpressionFragment target) {
        return new InvocationExpressionFragment(
            target,
            Method,
            Arguments
        );
    }

    public InvocationExpressionFragment WithArguments(params ImmutableArray<ExpressionFragment> arguments) {
        return new InvocationExpressionFragment(
            Target,
            Method,
            arguments
        );
    }

    public void Render(IRenderTreeBuilder renderTree) {
        if (Target is not null) {
            renderTree.Node(Target.Value);
            renderTree.Text(".");
        }

        renderTree.Node(Method);
        renderTree.Text("(");
        renderTree.SeparatedList(Arguments.AsSpan());
        renderTree.Text(")");
    }
}