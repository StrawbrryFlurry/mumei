namespace Mumei.CodeGen.Components;

internal sealed partial class QtSyntheticClassBuilder<TClassDef> {
    private Dictionary<string, ISyntheticType>? _dynamicallyBoundTypeInfos;

    public ISyntheticMethodBuilder<Delegate> DeclareMethod<TMethodDefinition>(
        SyntheticIdentifier name,
        Func<TMethodDefinition, Delegate> methodSelector,
        Action<TMethodDefinition> inputBinder
    ) where TMethodDefinition : SyntheticMethodDefinition, new() {
        var methodDef = new TMethodDefinition();
        inputBinder(methodDef);
        methodDef.BindDynamicComponents();
        var methodToBind = methodSelector(methodDef);
        methodDef.InternalBindCompilerMethod(this, methodToBind);
        return null!;
    }

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TMethodDefinition, TSignature>(
        SyntheticIdentifier name,
        Func<TMethodDefinition, TSignature> methodSelector,
        Action<TMethodDefinition> inputBinder
    ) where TMethodDefinition : SyntheticMethodDefinition, new() where TSignature : Delegate {
        throw new NotImplementedException();
    }

    public ISyntheticMethodBuilder<TSignature> DeclareMethod<TSignature>(SyntheticIdentifier name) where TSignature : Delegate {
        var method = new SyntheticMethodBuilder<TSignature>(
            name,
            this,
            ΦCompilerApi.Context
        );

        DeclareMethod(method);
        return method;
    }

    public void BindSyntheticImplementation(ISyntheticType member, ISyntheticType actualType) {
        throw new NotImplementedException();
    }

    public void BindSyntheticImplementation(Type member, ISyntheticType actualType) {
        throw new NotImplementedException();
    }

    public TField DeclareField<TField>(TField field) where TField : ISyntheticField {
        var fields = _fields ??= [];
        fields.Add(field);
        return field;
    }

    public ISyntheticFieldBuilder<TField> DeclareField<TField>(ISyntheticType type, SyntheticIdentifier name) {
        return DeclareField(new SyntheticField<TField>(
            null,
            AccessModifier.Private,
            type,
            name
        ));
    }

    public TProperty DeclareProperty<TProperty>(TProperty property) where TProperty : ISyntheticProperty {
        var properties = _properties ??= [];
        properties.Add(property);
        return property;
    }

    public ISyntheticPropertyBuilder<TProperty> DeclareProperty<TProperty>(
        ISyntheticType type,
        SyntheticIdentifier name,
        SyntheticPropertyAccessorList accessors
    ) {
        return DeclareProperty(new SyntheticProperty<TProperty>(
            null,
            accessors,
            AccessModifier.Private,
            type,
            name
        ));
    }

    public ISyntheticClassBuilder<TNew> WithDefinition<TNew>() where TNew : ISyntheticClass {
        throw new NotImplementedException();
    }

    public ISyntheticField<TField> DeclareField<TField>(SyntheticIdentifier name) {
        throw new NotImplementedException();
    }

    public ISyntheticField<CompileTimeUnknown> DeclareField(Type type, SyntheticIdentifier name) {
        throw new NotImplementedException();
    }


    public ISyntheticField<CompileTimeUnknown> DeclareField(ISyntheticType type, SyntheticIdentifier name) {
        throw new NotImplementedException();
    }

    public void DeclareConstructor<TImplementaiton>(Delegate impl) {
        throw new NotImplementedException();
    }

    public void Implement(Type baseType) {
        throw new NotImplementedException();
    }

    public void Implement(ISyntheticType baseType) {
        throw new NotImplementedException();
    }

    public void Extend(Type baseType) {
        throw new NotImplementedException();
    }

    public void Extend(ISyntheticType baseType) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TNested> DeclareNestedClass<TNested>(SyntheticIdentifier name) {
        var cls = new QtSyntheticClassBuilder<TNested>(name, this, context);
        WithNestedClass(cls);
        return cls;
    }

    public void Bind(Type t, ISyntheticType actualType, string bindingTargetExpression = "") {
        _dynamicallyBoundTypeInfos ??= new Dictionary<string, ISyntheticType>();
        _dynamicallyBoundTypeInfos[t.FullName!] = actualType;
    }
}