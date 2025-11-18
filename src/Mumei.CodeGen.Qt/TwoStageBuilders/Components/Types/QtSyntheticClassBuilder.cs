using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed partial class QtSyntheticClassBuilder<TClassDef>(SyntheticCompilation compilation) : ISyntheticClassBuilder<TClassDef> {
    private QtSyntheticAttributeList? _attributes;
    private List<ISyntheticMethod> _methods;
    private List<ISyntheticClass> _nestedClasses;

    public ISyntheticClassBuilder<TClassDef>.IλInternalClassBuilderCompilerApi λCompilerApi { get; }

    public TClassDef New(object[] args) {
        throw new NotImplementedException();
    }

    public TClassDef New(Func<TClassDef> constructorExpression) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TClassDef> WithName(string name) {
        throw new NotImplementedException();
    }

    public ISyntheticClassBuilder<TClassDef> WithModifiers(AccessModifierList modifiers) {
        throw new NotImplementedException();
    }

    public string MakeUniqueName(string n) {
        throw new NotImplementedException();
    }

    private void DeclareMethod(ISyntheticMethod method) {
        (_methods ??= []).Add(method);
    }

    private sealed class CompilerApi(SyntheticCompilation compilation, QtSyntheticClassBuilder<TClassDef> builder) : ISyntheticClassBuilder<TClassDef>.IλInternalClassBuilderCompilerApi {
        public SyntheticCompilation Compilation => compilation;

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