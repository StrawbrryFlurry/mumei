using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed partial class QtSyntheticClassBuilder<TClassDef>(QtSyntheticCompilation compilation) : ISyntheticClassBuilder<TClassDef> {
    private CompilerApi? _compilerApi;

    private string _name = compilation.λCompilerApi.MakeArbitraryUniqueName("UnnamedClass");

    private QtSyntheticAttributeList? _attributes;
    private List<ISyntheticMethod> _methods;
    private List<ISyntheticClass> _nestedClasses;

    private AccessModifierList _modifiers = AccessModifierList.Internal;

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
                modifiers,
                returnType,
                name,
                typeParameters,
                parameters,
                body
            );

            builder.DeclareMethod(method);
        }
    }
}