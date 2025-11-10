using System.Collections.Immutable;
using Mumei.CodeGen.Playground;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedClassDeclaration(
    ImmutableArray<SynthesizedAttribute> attributes,
    AccessModifier accessModifier,
    string name,
    SynthesizedTypeParameterList typeParameters,
    ImmutableArray<SynthesizedParameter> primaryConstructorParameters,
    ImmutableArray<SynthesizedTypeInfo> baseTypes,
    ImmutableArray<SynthesizedFieldDeclaration> fields,
    ImmutableArray<SynthesizedPropertyDeclaration> properties,
    ImmutableArray<SynthesizedMethodDeclaration> methods,
    ImmutableArray<IRenderer.IFeature> renderFeatures
) : IRenderNode {
    public static SynthesizedClassDeclaration Create(
        string name,
        ImmutableArray<SynthesizedAttribute> attributes = default,
        AccessModifier accessModifier = AccessModifier.Internal,
        SynthesizedTypeParameterList typeParameters = default,
        ImmutableArray<SynthesizedParameter> primaryConstructorParameters = default,
        ImmutableArray<SynthesizedTypeInfo> baseTypes = default,
        ImmutableArray<SynthesizedFieldDeclaration> fields = default,
        ImmutableArray<SynthesizedPropertyDeclaration> properties = default,
        ImmutableArray<SynthesizedMethodDeclaration> methods = default,
        ImmutableArray<IRenderer.IFeature> renderFeatures = default
    ) {
        return new SynthesizedClassDeclaration(
            attributes.EnsureInitialized(),
            accessModifier,
            name,
            typeParameters,
            primaryConstructorParameters.EnsureInitialized(),
            baseTypes.EnsureInitialized(),
            fields.EnsureInitialized(),
            properties.EnsureInitialized(),
            methods.EnsureInitialized(),
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

        for (var i = 0; i < methods.Length; i++) {
            var method = methods[i];
            renderTree.Node(method);
            if (i < methods.Length - 1) {
                renderTree.NewLine();
            }
        }

        renderTree.EndCodeBlock();
    }
}

public readonly struct SynthesizedMethodDeclaration(
    ImmutableArray<SynthesizedAttribute> attributes,
    AccessModifier accessModifier,
    SynthesizedTypeInfo returnType,
    string name,
    SynthesizedTypeParameterList typeParameters,
    ImmutableArray<SynthesizedParameter> parameters,
    SynthesizedCodeBlock body
) : IRenderNode {
    public string Name => name;

    public static SynthesizedMethodDeclaration Create(
        ImmutableArray<SynthesizedAttribute> attributes,
        AccessModifier accessModifier,
        SynthesizedTypeInfo returnType,
        string name,
        SynthesizedTypeParameterList typeParameters,
        ImmutableArray<SynthesizedParameter> parameters,
        SynthesizedCodeBlock body
    ) {
        return new SynthesizedMethodDeclaration(
            attributes.EnsureInitialized(),
            accessModifier,
            returnType,
            name,
            typeParameters,
            parameters.EnsureInitialized(),
            body
        );
    }


    public static SynthesizedMethodDeclaration Create<TBodyState>(
        ImmutableArray<SynthesizedAttribute> attributes,
        AccessModifier accessModifier,
        SynthesizedTypeInfo returnType,
        string name,
        SynthesizedTypeParameterList typeParameters,
        ImmutableArray<SynthesizedParameter> parameters,
        TBodyState bodyState,
        Action<TBodyState, IRenderTreeBuilder> declareBody
    ) {
        var body = SynthesizedCodeBlock.Create(bodyState, declareBody);

        return new SynthesizedMethodDeclaration(
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

        if (accessModifier.IsAbstract()) {
            renderTree.Text(";");
            return;
        }

        renderTree.Text(" ");
        renderTree.StartCodeBlock();
        renderTree.Node(body);
        renderTree.NewLine();
        renderTree.EndCodeBlock();
    }

    public SynthesizedMethodDeclaration WithName(string newName) {
        return new SynthesizedMethodDeclaration(
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

public readonly struct SynthesizedFieldDeclaration : IRenderNode {
    public void Render(IRenderTreeBuilder renderTree) { }
}

public readonly struct SynthesizedPropertyDeclaration : IRenderNode {
    public void Render(IRenderTreeBuilder renderTree) { }
}