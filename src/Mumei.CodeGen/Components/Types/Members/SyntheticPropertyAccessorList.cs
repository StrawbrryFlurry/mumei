using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

public sealed class SyntheticPropertyAccessorList {
    public SyntheticPropertyAccessorList(
        SyntheticPropertyAccessor? getter,
        SyntheticPropertyAccessor? setter
    ) {
        Getter = getter;
        Setter = setter;
    }

    public SyntheticPropertyAccessorList() { }

    public SyntheticPropertyAccessor? Getter { get; set; }
    public SyntheticPropertyAccessor? Setter { get; set; }
}

public sealed class SyntheticPropertyAccessor {
    public bool IsGetter { get; private init; }
    public bool IsSetter { get; private init; }
    public bool IsInit { get; private init; }

    public ISyntheticAttributeList Attributes { get; init; }
    public ISyntheticCodeBlock? Body { get; init; }

    public static SyntheticPropertyAccessor SimpleGetter { get; } = new() {
        IsGetter = true
    };

    public static SyntheticPropertyAccessor SimpleSetter { get; } = new() {
        IsSetter = true
    };

    public static SyntheticPropertyAccessor SimpleInit { get; } = new() {
        IsSetter = true,
        IsInit = true
    };

    private SyntheticPropertyAccessor() { }

    internal PropertyDeclarationFragment.AccessorFragment Construct(ICompilationUnitContext ctx) {
        var keyword = IsGetter
            ? "get"
            : IsSetter
                ? IsInit
                    ? "init"
                    : "set"
                : throw new InvalidOperationException();

        return new PropertyDeclarationFragment.AccessorFragment(
            [],
            new AccessModifierList(),
            keyword,
            ctx.SynthesizeOptional<CodeBlockFragment>(Body)
        );
    }
}