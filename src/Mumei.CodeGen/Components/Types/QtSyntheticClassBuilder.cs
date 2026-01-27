using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Common.Internal;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

internal sealed partial class QtSyntheticClassBuilder<TClassDef>(
    SyntheticIdentifier name,
    ISyntheticDeclaration parent,
    ICodeGenerationContext context
) : ISyntheticClassBuilder<TClassDef>,
    ISyntheticConstructable<ClassDeclarationFragment>,
    ISyntheticConstructable<NamespaceOrGlobalScopeFragment>,
    ISyntheticConstructable<TypeInfoFragment> {
    private CompilerApi? _compilerApi;

    public SyntheticIdentifier Name => _name;
    private SyntheticIdentifier _name = name;

    public ISyntheticDeclaration Parent { get; private set; } = parent;

    private ISyntheticAttributeList? _attributes;
    private List<ISyntheticClass>? _nestedClasses;
    private ISyntheticTypeParameterList? _typeParameters;

    private List<ISyntheticMethod>? _methods;
    private List<ISyntheticProperty>? _properties;
    private List<ISyntheticField>? _fields;

    private AccessModifierList _modifiers = AccessModifier.Internal;

    public IΦInternalClassBuilderCompilerApi ΦCompilerApi => _compilerApi ??= new CompilerApi(context, this);

    public TClassDef New(object[] args) {
        throw new NotImplementedException();
    }

    public TClassDef New(Func<TClassDef> constructorExpression) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TClassDef> WithName(SyntheticIdentifier name) {
        _name = name;
        return this;
    }

    public ISyntheticClassBuilder<TClassDef> WithAccessibility(AccessModifierList accessModifiers) {
        _modifiers = accessModifiers;
        return this;
    }

    public ISyntheticClassBuilder<TClassDef> WithDeclaration(ISyntheticDeclaration declaration) {
        Parent = declaration;
        return this;
    }

    public void WithNestedClass(ISyntheticClass nestedClass) {
        _nestedClasses ??= [];
        _nestedClasses.Add(nestedClass);
    }

    public void WithMethod(ISyntheticMethod method) {
        (_methods ??= []).Add(method);
    }

    public TMethod DeclareMethod<TMethod>(TMethod method) where TMethod : ISyntheticMethod {
        WithMethod(method);
        return method;
    }

    public ISyntheticFieldBuilder<CompileTimeUnknown> DeclareField(SyntheticIdentifier name) {
        throw new NotImplementedException();
    }

    public ISyntheticFieldBuilder<TField> DeclareField<TField>(SyntheticIdentifier name,
        ISyntheticExpression initialValue) {
        throw new NotImplementedException();
    }

    public SyntheticIdentifier MakeUniqueName(string name) {
        return SyntheticIdentifier.Unique(this, name);
    }

    public ISyntheticClassBuilder<TClassDef> WithTypeParameters(ISyntheticTypeParameterList typeParameters) {
        _typeParameters = typeParameters;
        return this;
    }

    private sealed class CompilerApi(ICodeGenerationContext context, QtSyntheticClassBuilder<TClassDef> builder)
        : IΦInternalClassBuilderCompilerApi {
        public ICodeGenerationContext Context => context;

        public void DeclareMethod(
            ISyntheticAttribute[] attributes,
            AccessModifierList modifiers,
            ISyntheticType returnType,
            SyntheticIdentifier name,
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

    public ClassDeclarationFragment Construct(ICompilationUnitContext compilationUnit) {
        var methods = new ArrayBuilder<MethodDeclarationFragment>();
        var fields = new ArrayBuilder<FieldDeclarationFragment>();
        var properties = new ArrayBuilder<PropertyDeclarationFragment>();
        var constructors = new ArrayBuilder<ConstructorDeclarationFragment>();
        var baseTypes = new ArrayBuilder<TypeInfoFragment>();

        var nestedClasses = new ArrayBuilder<ClassDeclarationFragment>();

        foreach (var synMethod in _methods ?? []) {
            var fragment = compilationUnit.Synthesize<MethodDeclarationFragment>(synMethod);
            methods.Add(fragment);
        }

        foreach (var synField in _fields ?? []) {
            var fragment = compilationUnit.Synthesize<FieldDeclarationFragment>(synField);
            fields.Add(fragment);
        }

        foreach (var synProperty in _properties ?? []) {
            var fragment = compilationUnit.Synthesize<PropertyDeclarationFragment>(synProperty);
            properties.Add(fragment);
        }

        foreach (var synNestedClass in _nestedClasses ?? []) {
            var fragment = compilationUnit.Synthesize<ClassDeclarationFragment>(synNestedClass);
            nestedClasses.Add(fragment);
        }

        return new ClassDeclarationFragment(
            compilationUnit.Synthesize(_attributes, AttributeListFragment.Empty),
            _modifiers,
            Name.Resolve(compilationUnit),
            compilationUnit.Synthesize(_typeParameters, TypeParameterListFragment.Empty),
            [],
            baseTypes.ToImmutableArrayAndFree(),
            constructors.ToImmutableArrayAndFree(),
            fields.ToImmutableArrayAndFree(),
            properties.ToImmutableArrayAndFree(),
            methods.ToImmutableArrayAndFree(),
            nestedClasses.ToImmutableArrayAndFree()
        );
    }

    NamespaceOrGlobalScopeFragment ISyntheticConstructable<NamespaceOrGlobalScopeFragment>.Construct(
        ICompilationUnitContext compilationUnit) {
        // TODO: Figure out how we can recursively create containers e.g. nested classes
        if (Parent is ISyntheticNamespace { IsGlobalNamespace: true }) {
            var globalFragment = NamespaceOrGlobalScopeFragment.GlobalScope;
            globalFragment = globalFragment.WithClassDeclarations([Construct(compilationUnit)]);
            return globalFragment;
        }

        if (Parent is ISyntheticNamespace parentNamespace) {
            var namespaceFragment = NamespaceOrGlobalScopeFragment.Create(
                parentNamespace.FullyQualifiedName,
                [Construct(compilationUnit)]
            );
            return namespaceFragment;
        }

        throw new NotImplementedException("Nested classes are not yet supported in QtSyntheticClassBuilder.");
    }

    TypeInfoFragment ISyntheticConstructable<TypeInfoFragment>.Construct(ICompilationUnitContext compilationUnit) {
        var name = Name.Resolve(compilationUnit);
        if (Parent is ISyntheticNamespace parentNamespace) {
            name = $"{parentNamespace.FullyQualifiedName}.{name}";
        }

        return new TypeInfoFragment(name);
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

    public bool Equals(ISyntheticType other) {
        return ReferenceEquals(this, other);
    }
}