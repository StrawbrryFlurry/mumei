using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.TwoStageBuilders.Components;
using Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt;

public sealed class SyntheticCompilationClassFactoryGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var codeFragmentInvocations = context.SyntaxProvider.CreateSyntaxProvider(
            IsCompilationDeclareClassInvocation,
            TranslateCodeFragmentInvocationProvider
        ).Where(x => x);

        var interceptorNamespace = codeFragmentInvocations.Collect().Select(CreateCodeFragmentInterceptorNamespace);

        context.RegisterSourceOutput(interceptorNamespace, (ctx, state) => {
            if (state.IsEmpty) {
                return;
            }

            var fileTree = new SourceFileRenderTreeBuilder();
            var content = fileTree.RenderRootNode(state);
            ctx.AddSource("SyntheticCompilationFactory.Class.g.cs", content);
        });
    }

    private static bool IsCompilationDeclareClassInvocation(SyntaxNode node, CancellationToken _) {
        return SyntaxNodeFilter.IsInvocationOf(node, nameof(SyntheticCompilation.DeclareClass), nameof(SyntheticCompilation), out var _, out var memberAccess)
               && memberAccess.Name is GenericNameSyntax;
    }

    private static InterceptInvocationIntermediateNode<ITypeSymbol> TranslateCodeFragmentInvocationProvider(GeneratorSyntaxContext context, CancellationToken _) {
        if (!SyntaxProviderFactory.TryCreateInvocation(context, out var invocation)) {
            return IntermediateNode.None;
        }

        if (!invocation.MethodInfo.IsDeclaredIn<SyntheticCompilation>()) {
            return IntermediateNode.None;
        }

        // In the future we might want to validate the input binder
        // and make sure it initializes all required properties.
        if (invocation.Operation.Arguments is not [not null, { Value: IDelegateCreationOperation { Syntax: LambdaExpressionSyntax } }]) {
            return IntermediateNode.None;
        }

        if (invocation.MethodInfo.TypeArguments.IsDefaultOrEmpty) {
            return IntermediateNode.None;
        }

        var classDefinition = invocation.MethodInfo.TypeArguments[0];
        return invocation.AsIntercept().WithState(classDefinition);
    }

    private static SynthesizedNamespace CreateCodeFragmentInterceptorNamespace(
        ImmutableArray<InterceptInvocationIntermediateNode<ITypeSymbol>> classDeclarationCalls,
        CancellationToken _
    ) {
        var compilation = new SyntheticCompilation(classDeclarationCalls[0].SemanticModel.Compilation);
        var interceptorClass = compilation.DeclareClass("SyntheticCompilationClassFactory")
            .WithModifiers(SyntheticAccessModifier.File, SyntheticAccessModifier.Static);

        foreach (var call in classDeclarationCalls) {
            DeclareInterceptorMethod(interceptorClass, call);
        }

        var ns = compilation.NamespaceFromCompilation("Generated")
            .WithMember(interceptorClass);

        return compilation.Synthesize(ns);
    }

    private static void DeclareInterceptorMethod(
        ISyntheticClassBuilder<CompileTimeUnknown> classBuilder,
        InterceptInvocationIntermediateNode<ITypeSymbol> declareClassInvocation
    ) {
        var binderClass = classBuilder.DeclareNestedClass<SyntheticClassDynamicMemberBinder<CompileTimeUnknown>>(
            $"DeclareClass__MemberBinder_{declareClassInvocation.State.Name}",
            binder => {
                binder.DefinitionClass = declareClassInvocation.State;
            }
        ).WithModifiers(SyntheticAccessModifier.Private, SyntheticAccessModifier.Sealed);

        var interceptMethod = classBuilder.DeclareInterceptorMethod(
            declareClassInvocation.Invocation,
            classBuilder.MakeUniqueName($"Intercept_DeclareClass__{declareClassInvocation.State.Name}}}")
        ).WithAccessibility(SyntheticAccessModifier.Private, SyntheticAccessModifier.Static);

        interceptMethod.WithBody(new SyntheticRenderCodeBlock(renderTree => {
            renderTree.Line("");
        }));
    }
}

file sealed class SyntheticClassDynamicMemberBinder<[Bindable] TClassDefinition>() : SyntheticClassDefinition<SyntheticClassDynamicMemberBinder<TClassDefinition>> {
    [Input]
    public ITypeSymbol DefinitionClass { get; set; }

    [Output]
    private readonly SyntheticCompilation _compilation;

    private readonly List<ISyntheticMethod<Action<ISyntheticClassBuilder<TClassDefinition>>>> _outputBinderMethods;

    [Output]
    public SyntheticClassDynamicMemberBinder(SyntheticCompilation compilation) : this() {
        _compilation = compilation;
    }

    public override void SetupDynamic(ISyntheticClassBuilder<SyntheticClassDynamicMemberBinder<TClassDefinition>> classBuilder) {
        classBuilder.Bind<TClassDefinition>(DefinitionClass);

        var outputAttribute = _compilation.TypeFromCompilation<OutputAttribute>();
        var outputMembers = DefinitionClass.GetMembers().Where(x => x.GetAttributes().Any(a => a.AttributeClass?.Equals(outputAttribute, SymbolEqualityComparer.Default) ?? false));

        foreach (var outputMember in outputMembers) {
            if (outputMember is IFieldSymbol outputField) {
                DeclareBindOutputField(classBuilder, outputField);
            }

            if (outputMember is IPropertySymbol outputProperty) {
                DeclareBindOutputProperty(classBuilder, outputProperty);
            }

            if (outputMember is IMethodSymbol outputMethod) {
                DeclareBindOutputMethod(classBuilder, outputMethod);
            }
        }
    }

    private void DeclareBindOutputField(ISyntheticClassBuilder<SyntheticClassDynamicMemberBinder<TClassDefinition>> classBuilder, IFieldSymbol field) {
        var bindMethod = classBuilder.DeclareMethod<Action<ISyntheticClassBuilder<TClassDefinition>>>($"BindOutput__field_{field.Name}");
        bindMethod.WithBody(new { Field = field }, static state => defBuilder => {
            defBuilder.DeclareField(state.Field.Type, state.Field.Name);
        });
        _outputBinderMethods.Add(bindMethod);
    }

    private void DeclareBindOutputProperty(ISyntheticClassBuilder<SyntheticClassDynamicMemberBinder<TClassDefinition>> classBuilder, IPropertySymbol property) {
        var bindMethod = classBuilder.DeclareMethod<Action<ISyntheticClassBuilder<TClassDefinition>>>($"BindOutput__property_{property.Name}");
        bindMethod.WithBody(new { Property = property }, static state => defBuilder => {
            defBuilder.DeclareProperty(state.Property.Type, state.Property.Name);
            // Handle getters and setters if needed
        });
        _outputBinderMethods.Add(bindMethod);
    }

    private void DeclareBindOutputMethod(ISyntheticClassBuilder<SyntheticClassDynamicMemberBinder<TClassDefinition>> classBuilder, IMethodSymbol method) {
        var bindMethod = classBuilder.DeclareMethod<Action<ISyntheticClassBuilder<TClassDefinition>>>($"BindOutput__method_{method.Name}");
        bindMethod.WithBody(new { Method = method }, static state => defBuilder => {
            // defBuilder.DeclareMethod(state.Method.Name);
        });

        _outputBinderMethods.Add(bindMethod);
    }

    [Output]
    public void BindOutputMembers(ISyntheticClassBuilder<TClassDefinition> classBuilder) {
        foreach (var bindOutputMethod in CompileTimeForEach(_outputBinderMethods)) {
            bindOutputMethod.Bind(this)(classBuilder);
        }
    }
}

file sealed class DeclareClassDefinitionMethod<[Bindable] TClassDefinition> : SyntheticInterceptorMethodDefinition where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
    [Input]
    public ISyntheticTypeInfo<SyntheticClassDynamicMemberBinder<TClassDefinition>> ClassDefinitionBinder { get; set; }

    [Input]
    public ITypeSymbol ClassDefinitionType { get; set; }

    public override void BindDynamicComponents(IMethodBuilder builder) {
        builder.BindSyntheticType<TClassDefinition>(ClassDefinitionType);
    }

    public ISyntheticClassBuilder<TClassDefinition> InterceptDeclareClass(
        SyntheticCompilation compilation,
        string name,
        Action<TClassDefinition> bindInputs
    ) {
        var definition = new TClassDefinition();
        bindInputs(definition);

        var classBuilder = compilation.λCompilerApi.DeclareClassBuilder<TClassDefinition>(name);
        definition.SetupDynamic(classBuilder);

        var definitionBinder = ClassDefinitionBinder.New(() => new SyntheticClassDynamicMemberBinder<TClassDefinition>(compilation));
        definitionBinder.BindOutputMembers(classBuilder);

        return classBuilder;
    }
}