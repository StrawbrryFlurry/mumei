using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components.Types;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components.Methods;

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

        public void ApplyMethodSignatureToBuilder<TSignature>(ISyntheticInterceptorMethodBuilder<TSignature> builder, MethodInfo methodInfo) where TSignature : Delegate {
            var parameterList = QtSyntheticParameterList.FromMethodInfo(methodInfo);
            builder.WithParameters(parameterList);

            var typeParameters = QtSyntheticTypeParameterList.FromMethodInfo(methodInfo);
            builder.WithTypeParameters(typeParameters);

            var returnType = compilation.GetType(methodInfo.ReturnType);
            builder.WithReturnType(returnType);
        }

        public void ApplyMethodSignatureToBuilder<TSignature>(ISyntheticInterceptorMethodBuilder<TSignature> builder, IMethodSymbol method) where TSignature : Delegate {
            var parameterList = QtSyntheticParameterList.FromMethodSymbol(method);
            builder.WithParameters(parameterList);

            var typeParameters = QtSyntheticTypeParameterList.FromMethodSymbol(method);
            builder.WithTypeParameters(typeParameters);

            var returnType = compilation.GetType(method.ReturnType);
            builder.WithReturnType(returnType);
        }

        public void ApplyConstructedMethodSignatureToBuilder<TSignature>(ISyntheticInterceptorMethodBuilder<TSignature> builder, IMethodSymbol method) where TSignature : Delegate {
            if (method.TypeParameters.IsEmpty) {
                ApplyMethodSignatureToBuilder(builder, method);
                return;
            }

            Debug.Assert(method.IsGenericMethod && method.TypeArguments.Length == method.TypeParameters.Length, "Method is not a constructed generic method.");
            var parameterList = QtSyntheticParameterList.FromMethodSymbol(method);
            builder.WithParameters(parameterList);

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