using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Common.Internal;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext {
    public ISyntheticCodeBlock Block(RenderFragment renderBlock) {
        return new QtSyntheticRenderCodeBlock(renderBlock);
    }

    public ISyntheticCodeBlock Block<TInput>(TInput input, Action<IRenderTreeBuilder, TInput> renderBlock) {
        return new QtSyntheticRenderCodeBlock<TInput>(new RenderFragment<TInput>(input, renderBlock));
    }

    public ISyntheticClassBuilder<TClassDefinition> DeclareClass<TClassDefinition>(
        SyntheticIdentifier name,
        Action<TClassDefinition> inputBinder
    ) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
        var classBuilder = ΦCompilerApi.DeclareClassBuilder<TClassDefinition>(name, null);
        var definition = new TClassDefinition();
        definition.CodeGenContext = this;
        inputBinder(definition);
        definition.Setup(classBuilder);
        definition.InternalBindCompilerOutputMembers(classBuilder);
        return classBuilder;
    }

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(SyntheticIdentifier name) {
        return new QtSyntheticClassBuilder<CompileTimeUnknown>(
            name,
            null,
            this
        );
    }

    public ISyntheticNamespaceBuilder Namespace(params ReadOnlySpan<string> namespaceSegments) {
        var s = new ArrayBuilder<char>(stackalloc char[ArrayBuilder.InitSize]);
        for (var i = 0; i < namespaceSegments.Length; i++) {
            if (i > 0) {
                s.Add('.');
            }

            s.AddRange(namespaceSegments[i]);
        }

        // TODO: should we create all namespace levels as well?
        var ns = new QtSyntheticNamespace(s.ToStringAndFree(), _globalNamespaceBuilder, ΦCompilerApi);
        _globalNamespaceBuilder.WithMember(ns);
        return ns;
    }

    public ISyntheticType Type(Type type) {
        return new RuntimeSyntheticType(type);
    }

    public ISyntheticParameter Parameter(ISyntheticType type, SyntheticIdentifier name, ParameterAttributes attributes) {
        return new SyntheticParameter(name, type, null, null, attributes);
    }

    public ISyntheticParameterList ParameterList(ReadOnlySpan<ISyntheticParameter> parameters) {
        return new SyntheticParameterList(parameters);
    }

    public ISyntheticTypeParameter TypeParameter(SyntheticIdentifier name) {
        return new QtSyntheticTypeParameter(name);
    }
}