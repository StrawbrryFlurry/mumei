using System.Collections.Immutable;
using Mumei.CodeGen.Playground;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedClass(
    AccessModifier accessModifier,
    string name,
    SynthesizedTypeParameterList typeParameters,
    ImmutableArray<SynthesizedField> fields,
    ImmutableArray<SynthesizedProperty> properties,
    ImmutableArray<SynthesizedMethod> methods,
    ImmutableArray<IRenderer.IFeature> renderFeatures
) : IRenderNode {
    public static SynthesizedClass Create(
        AccessModifier accessModifier,
        string name,
        SynthesizedTypeParameterList typeParameters,
        ImmutableArray<SynthesizedField> fields,
        ImmutableArray<SynthesizedProperty> properties,
        ImmutableArray<SynthesizedMethod> methods,
        ImmutableArray<IRenderer.IFeature> renderFeatures
    ) {
        return new SynthesizedClass(
            accessModifier,
            name,
            typeParameters,
            fields,
            properties,
            methods,
            renderFeatures
        );
    }

    public void Render(IRenderTreeBuilder renderTree) {
        foreach (var feature in renderFeatures) {
            renderTree.RequireFeature(feature);
        }

        renderTree.Interpolate(
            $"{accessModifier.List} class {name}"
        );


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

public readonly struct SynthesizedMethod(
    ImmutableArray<SynthesizedAttribute> attributes,
    AccessModifier accessModifier,
    SynthesizedTypeInfo returnType,
    string name,
    SynthesizedTypeParameterList typeParameters,
    ImmutableArray<SynthesizedParameter> parameters,
    SynthesizedCodeBlock body
) : IRenderNode {
    public string Name => name;

    public static SynthesizedMethod Create<TBodyState>(
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

        return new SynthesizedMethod(
            attributes,
            accessModifier,
            returnType,
            name,
            typeParameters,
            parameters,
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
}

public readonly struct SynthesizedField : IRenderNode {
    public void Render(IRenderTreeBuilder renderTree) { }
}

public readonly struct SynthesizedProperty : IRenderNode {
    public void Render(IRenderTreeBuilder renderTree) { }
}