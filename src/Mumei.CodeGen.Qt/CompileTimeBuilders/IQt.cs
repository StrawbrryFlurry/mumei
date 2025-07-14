using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.CodeGen.Playground;

internal interface IQt {
    public void InjectSyntax();
}

public interface IQtSyntaxProvider<TSyntax> where TSyntax : SyntaxNode {
    public TSyntax Syntax { get; }
}

public interface IQtSyntaxFunctor<TSyntax, TResult> where TSyntax : SyntaxNode {
    public abstract QtSyntaxTemplateItem<TResult> Invoke(IQtSyntaxProvider<TSyntax> provider);
}

public interface IQtSyntaxTemplateInterpolator { }

public class QtSyntaxTemplateItem<TActual>(
    SyntaxNode node
) {
    public static implicit operator TActual(QtSyntaxTemplateItem<TActual> item) {
        return default!;
    }
}

public static class TemplateResult {
    public static QtSyntaxTemplateItem<string> String(string s) {
        return new QtSyntaxTemplateItem<string>(
            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(s)
            )
        );
    }
}