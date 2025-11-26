using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed partial class QtSyntheticClassBuilder<TClassDef>(QtSyntheticCompilation compilation) : ISyntheticClassBuilder<TClassDef>, ISyntheticConstructable<ClassDeclarationFragment> {
    private CompilerApi? _compilerApi;

    public string Name => _name;

    private string _name = compilation.λCompilerApi.MakeArbitraryUniqueName("UnnamedClass");

    private ISyntheticAttributeList? _attributes;
    private List<ISyntheticMethod>? _methods;
    private List<ISyntheticClass>? _nestedClasses;
    private ISyntheticTypeParameterList? _typeParameters;

    private AccessModifierList _modifiers = AccessModifier.Internal;

    public IλInternalClassBuilderCompilerApi λCompilerApi => _compilerApi ??= new CompilerApi(compilation, this);

    public TClassDef New(object[] args) {
        throw new NotImplementedException();
    }

    public TClassDef New(Func<TClassDef> constructorExpression) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TClassDef> WithName(string name) {
        _name = name;
        return this;
    }

    public ISyntheticClassBuilder<TClassDef> WithModifiers(AccessModifierList modifiers) {
        _modifiers = modifiers;
        return this;
    }

    private UniqueNameGeneratorComponent? _uniqueNameGenerator;

    public string MakeUniqueName(string n) {
        return (_uniqueNameGenerator ??= new UniqueNameGeneratorComponent()).MakeUnique(n);
    }

    private void DeclareMethod(ISyntheticMethod method) {
        (_methods ??= []).Add(method);
    }

    private sealed class CompilerApi(ISyntheticCompilation compilation, QtSyntheticClassBuilder<TClassDef> builder) : IλInternalClassBuilderCompilerApi {
        public ISyntheticCompilation Compilation => compilation;

        public void DeclareMethod(
            ISyntheticAttribute[] attributes,
            AccessModifierList modifiers,
            ISyntheticType returnType,
            string name,
            ISyntheticTypeParameter[] typeParameters,
            ISyntheticParameter[] parameters,
            ISyntheticCodeBlock body
        ) {
            var method = new QtSyntheticMethod(
                attributes,
                modifiers + AccessModifier.Partial,
                returnType,
                name,
                typeParameters,
                parameters,
                body
            );

            builder.DeclareMethod(method);
        }
    }

    public ClassDeclarationFragment Construct(ISyntheticCompilation compilation) {
        var methods = new ArrayBuilder<MethodDeclarationFragment>();
        var fields = new ArrayBuilder<FieldDeclarationFragment>();
        var properties = new ArrayBuilder<PropertyDeclarationFragment>();
        var constructors = new ArrayBuilder<ConstructorDeclarationFragment>();
        var baseTypes = new ArrayBuilder<TypeInfoFragment>();

        foreach (var synMethod in _methods ?? []) {
            var fragment = compilation.Synthesize<MethodDeclarationFragment>(synMethod);
            methods.Add(fragment);
        }

        return new ClassDeclarationFragment(
            compilation.Synthesize(_attributes, AttributeListFragment.Empty),
            _modifiers,
            Name,
            compilation.Synthesize(_typeParameters, TypeParameterListFragment.Empty),
            [],
            baseTypes.ToImmutableArrayAndFree(),
            constructors.ToImmutableArrayAndFree(),
            fields.ToImmutableArrayAndFree(),
            properties.ToImmutableArrayAndFree(),
            methods.ToImmutableArrayAndFree(),
            [],
            []
        );
    }

    public ref SyntheticFieldRef<T> Field<T>(string name) {
        throw new NotImplementedException();
    }

    public ref SyntheticFieldRef<CompileTimeUnknown> Field(string name) {
        throw new NotImplementedException();
    }

    public SyntheticMethodRef<Delegate> Method(string name) {
        throw new NotImplementedException();
    }

    public SyntheticMethodRef<TSignature> Method<TSignature>(string name) where TSignature : Delegate {
        throw new NotImplementedException();
    }
}