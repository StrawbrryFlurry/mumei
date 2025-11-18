using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class QtSyntheticMethodBuilder<TSignature> : ISyntheticMethodBuilder<TSignature>, ISyntheticConstructable<MethodDeclarationFragment> where TSignature : Delegate {
    private static CompilerApi? _compilerApi;
    ISyntheticMethodBuilder<TSignature>.λIInternalMethodBuilderCompilerApi ISyntheticMethodBuilder<TSignature>.λCompilerApi => _compilerApi ??= new CompilerApi();

    public string Name { get; }

    private QtSyntheticAttributeList? _attributes;
    private AccessModifierList _accessModifiers = AccessModifierList.Empty;
    private QtSyntheticTypeParameterList? _typeParameters;
    private QtSyntheticParameterList? _parameters;
    private ISyntheticType _returnType;
    private ISyntheticCodeBlock _body;

    // TODO: Use MethodRef here?
    public TSignature Bind(object target) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public TDynamicSignature BindAs<TDynamicSignature>(object target) where TDynamicSignature : Delegate {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticMethodBuilder<TSignature> WithBody(TSignature bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticMethodBuilder<TSignature> WithBody<TDeps>(TDeps deps, Func<TDeps, TSignature> bodyImpl) {
        throw new CompileTimeComponentUsedAtRuntimeException();
    }

    public ISyntheticMethodBuilder<TSignature> WithParameters(params ISyntheticParameter[] parameters) {
        _parameters = new QtSyntheticParameterList(parameters);
        return this;
    }

    public ISyntheticMethodBuilder<TSignature> WithBody(ISyntheticCodeBlock body) {
        _body = body;
        return this;
    }

    public ISyntheticMethodBuilder<TSignature> WithAccessibility(AccessModifierList modifiers) {
        _accessModifiers = modifiers;
        return this;
    }

    private sealed class CompilerApi : ISyntheticMethodBuilder<TSignature>.λIInternalMethodBuilderCompilerApi {
        public ISyntheticCodeBlock CreateRendererCodeBlock(RenderFragment renderCodeBlock) {
            return new QtSyntheticRenderCodeBlock(renderCodeBlock);
        }

        public ISyntheticCodeBlock CreateRendererCodeBlock<TState>(RenderFragment<TState> renderCodeBlock) {
            return new QtSyntheticRenderCodeBlock<TState>(renderCodeBlock);
        }
    }

    public MethodDeclarationFragment Construct() {
        var attributes = _attributes?.Construct() ?? [];
        var accessModifiers = _accessModifiers;
        var typeParameters = _typeParameters?.Construct() ?? TypeParameterListFragment.Empty;

        if (_returnType is not ISyntheticConstructable<TypeInfoFragment> returnTypeConstructable) {
            throw new NotSupportedException();
        }

        var returnType = returnTypeConstructable.Construct();
        var parameterList = _parameters?.Construct() ?? [];
        if (_body is not ISyntheticConstructable<CodeBlockFragment> bodyConstructable) {
            throw new NotSupportedException();
        }

        var body = bodyConstructable.Construct();

        return MethodDeclarationFragment.Create(
            attributes,
            accessModifiers,
            typeParameters,
            returnType,
            Name,
            parameterList,
            body
        );
    }
}