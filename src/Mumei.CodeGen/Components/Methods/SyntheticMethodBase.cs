using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal abstract class SyntheticMethodBase<TBuilder>(ISyntheticIdentifier name, IΦInternalClassBuilderCompilerApi classApi) : ISyntheticConstructable<MethodDeclarationFragment> where TBuilder : class {
    protected IΦInternalMethodBuilderCompilerApi CompilerApi => field ??= new CompilerApiImpl(classApi.Context);
    public IΦInternalMethodBuilderCompilerApi ΦCompilerApi => CompilerApi;

    public ISyntheticIdentifier Name { get; protected set; } = name;

    public ISyntheticParameterList Parameters => _parameters ??= new QtSyntheticParameterList([]);

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
        Name = new ConstantSyntheticIdentifier(name);
        return Builder;
    }

    public TBuilder WithName(ISyntheticIdentifier name) {
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
        _attributes = new SyntheticAttributeList(attributes);
        return Builder;
    }

    public MethodDeclarationFragment Construct(ICompilationUnitContext compilationUnit) {
        EnsureValid();

        var accessModifiers = _accessModifiers;

        var attributes = compilationUnit.Synthesize<ImmutableArray<AttributeFragment>>(_attributes, []);
        var typeParameters = compilationUnit.Synthesize<TypeParameterListFragment>(_typeParameters, []);
        var returnType = compilationUnit.Synthesize<TypeInfoFragment>(_returnType);
        var parameterList = compilationUnit.Synthesize<ImmutableArray<ParameterFragment>>(_parameters, []);
        var body = compilationUnit.Synthesize<CodeBlockFragment>(_body);

        return MethodDeclarationFragment.Create(
            attributes,
            accessModifiers,
            typeParameters,
            returnType,
            Name.Resolve(compilationUnit.IdentifierResolver),
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

    private sealed class CompilerApiImpl(ICodeGenerationContext context) : IΦInternalMethodBuilderCompilerApi {
        public ICodeGenerationContext Context => context;
    }
}