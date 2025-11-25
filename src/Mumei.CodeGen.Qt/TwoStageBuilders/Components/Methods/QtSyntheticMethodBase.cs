using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal abstract class QtSyntheticMethodBase<TBuilder>(IλInternalClassBuilderCompilerApi classApi) : ISyntheticConstructable<MethodDeclarationFragment> where TBuilder : class {
    private static CompilerApiImpl? _compilerApi;
    protected λIInternalMethodBuilderCompilerApi CompilerApi => _compilerApi ??= new CompilerApiImpl(classApi.Compilation);
    public λIInternalMethodBuilderCompilerApi λCompilerApi => CompilerApi;

    public string Name { get; protected set; } = classApi.Compilation.MakeUniqueName("UnnamedMethod");

    protected ISyntheticAttributeList? _attributes;
    protected AccessModifierList _accessModifiers = AccessModifierList.Empty;
    protected ISyntheticTypeParameterList? _typeParameters;
    protected ISyntheticParameterList? _parameters;
    protected ISyntheticType _returnType;
    protected ISyntheticCodeBlock _body;

    private TBuilder Builder => Unsafe.As<TBuilder>(this);

    public TBuilder WithName(string name) {
        Name = name;
        return Builder;
    }

    public TBuilder WithAccessibility(AccessModifierList modifiers) {
        _accessModifiers = modifiers;
        return Builder;
    }

    public TBuilder WithParameters(params ReadOnlySpan<ISyntheticParameter> parameters) {
        _parameters = new QtSyntheticParameterList(parameters.ToArray());
        return Builder;
    }

    public TBuilder WithParameters(ISyntheticParameterList parameterList) {
        _parameters = parameterList;
        return Builder;
    }

    public TBuilder WithTypeParameters(ISyntheticTypeParameterList typeParameterList) {
        return Builder;
    }

    public TBuilder WithTypeParameters(params ReadOnlySpan<ISyntheticTypeParameter> typeParameterList) {
        return Builder;
    }

    public TBuilder WithReturnType(ISyntheticType returnType) {
        _returnType = returnType;
        return Builder;
    }

    public MethodDeclarationFragment Construct(ISyntheticCompilation compilation) {
        var accessModifiers = _accessModifiers;

        var attributes = FragmentConstructor.ConstructFragment<ImmutableArray<AttributeFragment>>(compilation, _attributes, []);
        var typeParameters = FragmentConstructor.ConstructFragment<TypeParameterListFragment>(compilation, _typeParameters, []);
        var returnType = FragmentConstructor.ConstructFragment<TypeInfoFragment>(compilation, _returnType);
        var parameterList = FragmentConstructor.ConstructFragment<ImmutableArray<ParameterFragment>>(compilation, _parameters, []);
        var body = FragmentConstructor.ConstructFragment<CodeBlockFragment>(compilation, _body);

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

    private sealed class CompilerApiImpl(ISyntheticCompilation compilation) : λIInternalMethodBuilderCompilerApi {
        public void ApplyMethodSignatureToBuilder<TSignature>(ISyntheticMethodBuilder<TSignature> builder, MethodInfo methodInfo) where TSignature : Delegate {
            var parameterList = QtSyntheticParameterList.FromMethodInfo(methodInfo);
            builder.WithParameters(parameterList);

            var typeParameters = QtSyntheticTypeParameterList.FromMethodInfo(methodInfo);
            builder.WithTypeParameters(typeParameters);

            var returnType = compilation.GetType(methodInfo.ReturnType);
            builder.WithReturnType(returnType);
        }

        public void ApplyMethodSignatureToBuilder<TSignature>(ISyntheticMethodBuilder<TSignature> builder, IMethodSymbol method) where TSignature : Delegate {
            var parameterList = QtSyntheticParameterList.FromMethodSymbol(method);
            builder.WithParameters(parameterList);

            var typeParameters = QtSyntheticTypeParameterList.FromMethodSymbol(method);
            builder.WithTypeParameters(typeParameters);

            var returnType = compilation.GetType(method.ReturnType);
            builder.WithReturnType(returnType);
        }

        public ISyntheticCodeBlock CreateRendererCodeBlock(RenderFragment renderCodeBlock) {
            return new QtSyntheticRenderCodeBlock(renderCodeBlock);
        }

        public ISyntheticCodeBlock CreateRendererCodeBlock<TState>(RenderFragment<TState> renderCodeBlock) {
            return new QtSyntheticRenderCodeBlock<TState>(renderCodeBlock);
        }
    }
}