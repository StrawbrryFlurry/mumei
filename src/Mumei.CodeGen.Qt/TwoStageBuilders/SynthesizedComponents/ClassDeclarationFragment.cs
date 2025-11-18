using System.Collections.Immutable;
using Mumei.CodeGen.Playground;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct ClassDeclarationFragment(
    ImmutableArray<AttributeFragment> attributes,
    AccessModifier accessModifier,
    string name,
    TypeParameterListFragment typeParameters,
    ImmutableArray<ParameterFragment> primaryConstructorParameters,
    ImmutableArray<TypeInfoFragment> baseTypes,
    ImmutableArray<FieldDeclarationFragment> fields,
    ImmutableArray<PropertyDeclarationFragment> properties,
    ImmutableArray<MethodDeclarationFragment> methods,
    ImmutableArray<ClassDeclarationFragment> nestedClassDeclarations,
    ImmutableArray<IRenderer.IFeature> renderFeatures
) : IRenderFragment {
    public static ClassDeclarationFragment Create(
        string name,
        ImmutableArray<AttributeFragment> attributes = default,
        AccessModifier accessModifier = AccessModifier.Internal,
        TypeParameterListFragment typeParameters = default,
        ImmutableArray<ParameterFragment> primaryConstructorParameters = default,
        ImmutableArray<TypeInfoFragment> baseTypes = default,
        ImmutableArray<FieldDeclarationFragment> fields = default,
        ImmutableArray<PropertyDeclarationFragment> properties = default,
        ImmutableArray<MethodDeclarationFragment> methods = default,
        ImmutableArray<ClassDeclarationFragment> nestedClassDeclarations = default,
        ImmutableArray<IRenderer.IFeature> renderFeatures = default
    ) {
        return new ClassDeclarationFragment(
            attributes.EnsureInitialized(),
            accessModifier,
            name,
            typeParameters,
            primaryConstructorParameters.EnsureInitialized(),
            baseTypes.EnsureInitialized(),
            fields.EnsureInitialized(),
            properties.EnsureInitialized(),
            methods.EnsureInitialized(),
            nestedClassDeclarations.EnsureInitialized(),
            renderFeatures.EnsureInitialized()
        );
    }

    public void Render(IRenderTreeBuilder renderTree) {
        foreach (var feature in renderFeatures) {
            renderTree.RequireFeature(feature);
        }

        if (!attributes.IsEmpty) {
            renderTree.List(attributes.AsSpan());
            renderTree.NewLine();
        }

        renderTree.Interpolate(
            $"{accessModifier.List} class {name}{typeParameters.List}"
        );

        if (!primaryConstructorParameters.IsEmpty) {
            renderTree.Text("(");
            renderTree.SeparatedList(primaryConstructorParameters.AsSpan());
            renderTree.Text(")");
        }

        if (!baseTypes.IsEmpty) {
            renderTree.Text(" : ");
            renderTree.SeparatedList(baseTypes.AsSpan(), static x => x.FullName);
        }

        renderTree.Node(typeParameters.Constraints);

        renderTree.Text(" ");
        renderTree.StartCodeBlock();

        foreach (var field in fields) {
            // field.WriteSyntax(ref writer);
            // writer.WriteLine();
        }

        renderTree.List(properties.AsSpan());
        renderTree.List(fields.AsSpan());
        renderTree.List(methods.AsSpan());
        renderTree.List(nestedClassDeclarations.AsSpan());

        renderTree.EndCodeBlock();
    }
}

public readonly struct MethodDeclarationFragment(
    ImmutableArray<AttributeFragment> attributes,
    AccessModifierList accessModifier,
    TypeInfoFragment returnType,
    string name,
    TypeParameterListFragment typeParameters,
    ImmutableArray<ParameterFragment> parameters,
    CodeBlockFragment body
) : IRenderFragment {
    public string Name => name;

    public static MethodDeclarationFragment Create(
        ImmutableArray<AttributeFragment> attributes,
        AccessModifierList accessModifier,
        TypeParameterListFragment typeParameters,
        TypeInfoFragment returnType,
        string name,
        ImmutableArray<ParameterFragment> parameters,
        CodeBlockFragment body
    ) {
        return new MethodDeclarationFragment(
            attributes.EnsureInitialized(),
            accessModifier,
            returnType,
            name,
            typeParameters,
            parameters.EnsureInitialized(),
            body
        );
    }


    public static MethodDeclarationFragment Create<TBodyState>(
        ImmutableArray<AttributeFragment> attributes,
        AccessModifierList accessModifier,
        TypeParameterListFragment typeParameters,
        TypeInfoFragment returnType,
        string name,
        ImmutableArray<ParameterFragment> parameters,
        TBodyState bodyState,
        Action<IRenderTreeBuilder, TBodyState> declareBody
    ) {
        var body = CodeBlockFragment.Create(bodyState, declareBody);

        return new MethodDeclarationFragment(
            attributes.EnsureInitialized(),
            accessModifier,
            returnType,
            name,
            typeParameters,
            parameters.EnsureInitialized(),
            body
        );
    }

    public void Render(IRenderTreeBuilder renderTree) {
        if (!attributes.IsEmpty) {
            renderTree.List(attributes.AsSpan());
            renderTree.NewLine();
        }

        renderTree.Interpolate($"{accessModifier.List} {returnType.FullName} {name}{typeParameters.List}");

        renderTree.Text("(");
        renderTree.SeparatedList(parameters.AsSpan());
        renderTree.Text(")");

        renderTree.Node(typeParameters.Constraints);

        if (accessModifier.IsAbstract) {
            renderTree.Text(";");
            return;
        }

        renderTree.Text(" ");
        renderTree.StartCodeBlock();
        renderTree.Node(body);
        renderTree.EndCodeBlock();
    }

    public MethodDeclarationFragment WithName(string newName) {
        return new MethodDeclarationFragment(
            attributes,
            accessModifier,
            returnType,
            newName,
            typeParameters,
            parameters,
            body
        );
    }
}

public readonly struct FieldDeclarationFragment : IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree) { }
}

public readonly struct PropertyDeclarationFragment(
    ImmutableArray<AttributeFragment> attributes,
    AccessModifierList accessModifier,
    TypeInfoFragment type,
    string name,
    PropertyDeclarationFragment.AccessorFragment? getAccessor,
    PropertyDeclarationFragment.AccessorFragment? setOrInitAccessor
) : IRenderFragment {
    public readonly ImmutableArray<AttributeFragment> Attributes = attributes.EnsureInitialized();
    public readonly TypeInfoFragment Type = type;
    public readonly string Name = name;
    public readonly AccessorFragment? GetAccessor = getAccessor;
    public readonly AccessorFragment? SetOrInitAccessor = setOrInitAccessor;

    public void Render(IRenderTreeBuilder renderTree) {
        if (!Attributes.IsEmpty) {
            renderTree.List(attributes.AsSpan());
            renderTree.NewLine();
        }

        renderTree.Interpolate($"{accessModifier.List} {Type.FullName} {Name} ");
        renderTree.StartCodeBlock();

        if (GetAccessor is { } getAcc) {
            renderTree.Node(getAcc);
            renderTree.NewLine();
        }

        if (SetOrInitAccessor is { } setAcc) {
            renderTree.Node(setAcc);
            renderTree.NewLine();
        }

        renderTree.EndCodeBlock();
    }

    public readonly struct AccessorFragment(
        ImmutableArray<AttributeFragment> attributes,
        AccessModifierList? accessModifier,
        string keyword,
        CodeBlockFragment? block
    ) : IRenderFragment {
        public static AccessorFragment Get(
            AccessModifierList? accessModifier = null,
            CodeBlockFragment? block = null
        ) {
            return new AccessorFragment([], accessModifier, "get", block);
        }

        public static AccessorFragment Set(
            AccessModifierList? accessModifier = null,
            CodeBlockFragment? block = null
        ) {
            return new AccessorFragment([], accessModifier, "set", block);
        }

        public static AccessorFragment Init(
            AccessModifierList? accessModifier = null,
            CodeBlockFragment? block = null
        ) {
            return new AccessorFragment([], accessModifier, "init", block);
        }

        public void Render(IRenderTreeBuilder renderTree) {
            if (!attributes.IsEmpty) {
                renderTree.List(attributes.AsSpan());
                renderTree.NewLine();
            }

            if (accessModifier is { } propertyAccessModifier) {
                renderTree.Interpolate($"{propertyAccessModifier.List} ");
            }

            renderTree.Text(keyword);

            if (block is { } accessorBlock) {
                renderTree.Text(" ");
                renderTree.StartCodeBlock();
                renderTree.Node(accessorBlock);
                renderTree.EndCodeBlock();
            } else {
                renderTree.Text(";");
            }
        }
    }

    public static PropertyDeclarationFragment Create(
        AccessModifierList accessModifier,
        TypeInfoFragment type,
        string name,
        AccessorFragment getAccessor
    ) {
        return new PropertyDeclarationFragment(
            [],
            accessModifier,
            type,
            name,
            getAccessor,
            null
        );
    }
}