using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

internal sealed partial class QtSyntheticClassBuilder<TClassDef>(
    SyntheticIdentifier name,
    ISyntheticDeclaration parent,
    ICodeGenerationContext context
) : ISyntheticClassBuilder<TClassDef>, ISyntheticConstructable<ClassDeclarationFragment>, ISyntheticConstructable<NamespaceOrGlobalScopeFragment> {
    private CompilerApi? _compilerApi;

    public SyntheticIdentifier Name => _name;
    private SyntheticIdentifier _name = name;

    public ISyntheticDeclaration Parent { get; } = parent;

    private ISyntheticAttributeList? _attributes;
    private List<ISyntheticMethod>? _methods;
    private List<ISyntheticClass>? _nestedClasses;
    private ISyntheticTypeParameterList? _typeParameters;

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

    public ISyntheticClassBuilder<TClassDef> DeclareMethod(ISyntheticMethod method) {
        (_methods ??= []).Add(method);
        return this;
    }

    public SyntheticIdentifier MakeUniqueName(string name) {
        return SyntheticIdentifier.Unique(this, name);
    }

    private sealed class CompilerApi(ICodeGenerationContext context, QtSyntheticClassBuilder<TClassDef> builder) : IΦInternalClassBuilderCompilerApi {
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

        foreach (var synMethod in _methods ?? []) {
            var fragment = compilationUnit.Synthesize<MethodDeclarationFragment>(synMethod);
            methods.Add(fragment);
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
            []
        );
    }

    NamespaceOrGlobalScopeFragment ISyntheticConstructable<NamespaceOrGlobalScopeFragment>.Construct(ICompilationUnitContext compilationUnit) {
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