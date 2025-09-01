using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.Roslyn;
using Mumei.CodeGen.Qt.Tests.Setup;
using Mumei.Roslyn.Testing.Comp;
using SourceCodeFactory;

namespace Mumei.CodeGen.Qt.Tests.CompileTimeBuilders;

public sealed class QtMethodTemplateDeclarationVisitorTests {
    [Fact]
    public void Test() {
        var compilation = new TestCompilationBuilder().AddReference(
            SourceCode.Of<Templates.MethodTemplate>()
        ).Build();

        ImmutableHashSet<string> stateIdentifiers = ["explicitState", "implicitState"];
        ImmutableHashSet<string> parameterIdentifiers = ["param1", "param2"];

        var method = compilation.GetTypeMemberSymbol<IMethodSymbol>($"Mumei.CodeGen.Qt.Tests.CompileTimeBuilders.{nameof(Templates.MethodTemplate)}", nameof(Templates.MethodTemplate.TemplateMethod));
        var declaringType = method.ContainingType;
        var methodSyntax = method.DeclaringSyntaxReferences[0].GetSyntax();
        var visitor = new QtMethodTemplateDeclarationVisitor(
            compilation.GetSemanticModel(methodSyntax.SyntaxTree),
            declaringType,
            stateIdentifiers,
            parameterIdentifiers
        );

        var updated = visitor.Visit(methodSyntax);
        var actual = updated.NormalizeWhitespace().ToFullString();
        SyntaxVerifier.Verify(
            actual,
            $$"""
              public void TemplateMethod(string param1, object param2)
              {
                  // TODO: This should be an analyzer warning
                  // var @this = this;
                  int implicitCast = {{This()}}.Length;
                  int explicitCast = {{This()}}.Length;
                  bool x = string.IsNullOrEmpty({{Param("param1")}});
                  bool y = x.Equals({{Param("param2")}});
                  {{Member("InstanceMethodWithThis")}}({{Param("param1")}}, {{Param("param2")}});
                  {{Member("ImplicitInstanceMethod")}}({{Param("param1")}}, {{Param("param2")}});
                  {{Member("_explicitField")}} = {{Param("param1")}};
                  {{Member("_implicitField")}} = {{Param("param2")}};
                  {{Member("ExplicitProperty")}} = {{State("explicitState")}};
                  {{Member("ImplicitProperty")}} = {{State("implicitState")}};
                  {{typeof(MethodInfo)}} implicitMethod = {{TemplateMarker(ProxyMethodBindingKeys.MethodInfo)}};
                  {{typeof(MethodInfo)}} explicitMethod = {{TemplateMarker(ProxyMethodBindingKeys.MethodInfo)}};
                  var implicitInvocationArgs = {{TemplateMarker(ProxyMethodBindingKeys.ArgumentList)}};
                  var explicitInvocationArgs = {{TemplateMarker(ProxyMethodBindingKeys.ArgumentList)}};
                  object implicitFirstArgument = {{TemplateMarker(ProxyMethodBindingKeys.ArgumentList)}}[0];
                  object implicitInvocationResult = {{TemplateMarker(ProxyMethodBindingKeys.Invoke)}};
                  object explicitInvocationResult = {{TemplateMarker(ProxyMethodBindingKeys.Invoke)}};
                  string implicitInvocationMember = {{TemplateMarker(ProxyMethodBindingKeys.Invoke)}}.ToString();
              }
              """
        );

        static string Member(string name) {
            var key = TemplateBindingKey.For(ProxyMethodBindingKeys.Member, name);
            return __DynamicallyBoundSourceCode.MakeDynamicSection(key);
        }

        static string This() {
            var key = TemplateBindingKey.For(ProxyMethodBindingKeys.This);
            return __DynamicallyBoundSourceCode.MakeDynamicSection(key);
        }

        static string State(string name) {
            var key = TemplateBindingKey.For(ProxyMethodBindingKeys.State, name);
            return __DynamicallyBoundSourceCode.MakeDynamicSection(key);
        }

        static string Param(string name) {
            var key = TemplateBindingKey.For(ProxyMethodBindingKeys.Parameter, name);
            return __DynamicallyBoundSourceCode.MakeDynamicSection(key);
        }

        static string TemplateMarker(string kind) {
            var key = TemplateBindingKey.For(kind);
            return __DynamicallyBoundSourceCode.MakeDynamicSection(key);
        }
    }
}

// ReSharper disable ArrangeThisQualifier
file sealed class Templates {
    public sealed class MethodTemplate(string explicitState, object implicitState) : QtInterceptorMethodTemplate {
        public required string ExplicitProperty { get; set; }
        public required object ImplicitProperty { get; set; }

        private string _explicitField;
        private object _implicitField;

        public void TemplateMethod(string param1, object param2) {
            // TODO: This should be an analyzer warning
            // var @this = this;
            var implicitCast = Is<string>().Length;
            var explicitCast = this.Is<string>().Length;

            var x = string.IsNullOrEmpty(param1);
            var y = x.Equals(param2);

            this.InstanceMethodWithThis(param1, param2);
            ImplicitInstanceMethod(param1, param2);

            this._explicitField = param1;
            _implicitField = param2;

            this.ExplicitProperty = explicitState;
            ImplicitProperty = implicitState;

            var implicitMethod = Method;
            var explicitMethod = this.Method;

            var implicitInvocationArgs = InvocationArguments;
            var explicitInvocationArgs = this.InvocationArguments;
            var implicitFirstArgument = this.InvocationArguments[0];

            var implicitInvocationResult = Invoke<object>();
            var explicitInvocationResult = this.Invoke<object>();
            var implicitInvocationMember = Invoke<object>().ToString();
        }

        public void InstanceMethodWithThis(string param1, object param2) { }

        public void ImplicitInstanceMethod(string param1, object param2) { }
    }
}