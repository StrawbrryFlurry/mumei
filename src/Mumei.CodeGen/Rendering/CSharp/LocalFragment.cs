using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct LocalFragment(TypeInfoFragment? type, ExpressionFragment name) : IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree) {
        if (type is { } localType) {
            renderTree.Interpolate($"{localType.FullName} ");
        }

        renderTree.Node(name);
    }

    public static LocalFragment Create(
        ExpressionFragment name
    ) {
        return new LocalFragment(null, name);
    }

    public static LocalFragment Create(
        TypeInfoFragment type,
        ExpressionFragment name
    ) {
        return new LocalFragment(type, name);
    }

    public static LocalFragment Var(
        ExpressionFragment name,
        out ExpressionFragment localName
    ) {
        localName = name;
        var local = new LocalFragment(TypeInfoFragment.Var, name);
        return local;
    }

    public static LocalFragment Create(
        TypeInfoFragment type,
        ExpressionFragment name,
        out LocalFragment local
    ) {
        local = new LocalFragment(type, name);
        return local;
    }

    public static LocalFragment Create<T>(
        ExpressionFragment name
    ) {
        return new LocalFragment(new TypeInfoFragment(typeof(T)), name);
    }

    public static LocalFragment Create(
        ITypeSymbol type,
        ExpressionFragment name
    ) {
        return new LocalFragment(new TypeInfoFragment(type), name);
    }
}