namespace Mumei.CodeGen.Rendering.CSharp;

public static class FragmentFactory {
    public static LocalFragment Var(
        ExpressionFragment name,
        out ExpressionFragment localName
    ) {
        localName = name;
        var local = new LocalFragment(TypeInfoFragment.Var, name);
        return local;
    }
}