using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal abstract class QtSyntheticMethodBase<TBuilder>(string name, IλInternalClassBuilderCompilerApi classApi) : ISyntheticConstructable<MethodDeclarationFragment> where TBuilder : class {
    private CompilerApiImpl? _compilerApi;
    protected λIInternalMethodBuilderCompilerApi CompilerApi => _compilerApi ??= new CompilerApiImpl(classApi.Compilation);
    public λIInternalMethodBuilderCompilerApi λCompilerApi => CompilerApi;

    public string Name { get; protected set; } = name;

    // ReSharper disable InconsistentNaming
    protected ISyntheticAttributeList? _attributes;
    protected AccessModifierList _accessModifiers = AccessModifierList.Empty;
    protected ISyntheticTypeParameterList? _typeParameters;
    protected ISyntheticParameterList? _parameters;
    protected ISyntheticType? _returnType;
    protected ISyntheticCodeBlock? _body;
    // ReSharper restore InconsistentNaming

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
        _typeParameters = typeParameterList;
        return Builder;
    }

    public TBuilder WithTypeParameters(params ReadOnlySpan<ISyntheticTypeParameter> typeParameterList) {
        _typeParameters = new QtSyntheticTypeParameterList(typeParameterList);
        return Builder;
    }

    public TBuilder WithReturnType(ISyntheticType returnType) {
        _returnType = returnType;
        return Builder;
    }

    public TBuilder WithBody(ISyntheticCodeBlock body) {
        _body = body;
        return Builder;
    }

    public TBuilder WithAttributes(ISyntheticAttributeList attributes) {
        _attributes = attributes;
        return Builder;
    }

    public TBuilder WithAttributes(params ReadOnlySpan<ISyntheticAttribute> attributes) {
        _attributes = new QtSyntheticAttributeList(attributes);
        return Builder;
    }

    public MethodDeclarationFragment Construct(ISyntheticCompilation compilation) {
        EnsureValid();

        var accessModifiers = _accessModifiers;

        var attributes = compilation.Synthesize<ImmutableArray<AttributeFragment>>(_attributes, []);
        var typeParameters = compilation.Synthesize<TypeParameterListFragment>(_typeParameters, []);
        var returnType = compilation.Synthesize<TypeInfoFragment>(_returnType);
        var parameterList = compilation.Synthesize<ImmutableArray<ParameterFragment>>(_parameters, []);
        var body = compilation.Synthesize<CodeBlockFragment>(_body);

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

    private void EnsureValid() {
        // TODO: We should report these in the compilation as diagnostics rather than throwing exceptions.
        if (_returnType is null) {
            throw new InvalidOperationException("Method must have a return type.");
        }

        if (_body is null && !_accessModifiers.Contains(AccessModifier.Abstract)) {
            throw new InvalidOperationException("Non-Abstract methods must declare a body.");
        }

        if (_body is not null && _accessModifiers.Contains(AccessModifier.Abstract)) {
            throw new InvalidOperationException("Abstract methods cannot declare a body.");
        }
    }

    private sealed class CompilerApiImpl(ISyntheticCompilation compilation) : λIInternalMethodBuilderCompilerApi {
        public ISyntheticCodeBlock CreateRendererCodeBlock(RenderFragment renderCodeBlock) {
            return new QtSyntheticRenderCodeBlock(renderCodeBlock);
        }

        public ISyntheticCodeBlock CreateRendererCodeBlock<TState>(RenderFragment<TState> renderCodeBlock) {
            return new QtSyntheticRenderCodeBlock<TState>(renderCodeBlock);
        }
    }
}