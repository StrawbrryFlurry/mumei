using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;
using Mumei.Common.Reflection;
using Mumei.Core;
using Mumei.DependencyInjection.Roslyn.Module;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn;

internal class Generator {
  public List<ModuleDeclaration> Declarations { get; } = new();

  public void Run() {
    foreach (var moduleDeclaration in Declarations) {
      var moduleBuilder = new FileBuilder($"{moduleDeclaration.Name}.module.g.cs");
      var classBuilder = moduleBuilder.AddClassDeclaration(
        moduleDeclaration.Name,
        SyntaxVisibility.Public | SyntaxVisibility.Sealed
      );

      classBuilder.AddInterfaceImplementation<IModule>();
      classBuilder.AddInterfaceImplementation(moduleDeclaration.InterfaceDeclaration);

      classBuilder.AddField<IInjector>("Parent");

      foreach (var provider in moduleDeclaration.Providers) {
        AddModuleProvider(classBuilder, provider, moduleDeclaration);
      }
    }
  }

  public void AddConfigureMethod(
    ClassSyntaxBuilder cs,
    FileBuilder fileBuilder,
    ModuleProviderConfiguration providerConfiguration
  ) {
    var moduleType = providerConfiguration.Declaration.InterfaceDeclaration;
    var configureMethodName = new MumeiStringExpression(providerConfiguration.ConfigurationMethod.Name);

    cs.AddField<MethodInfo>("ConfigureλConfigureHttpClientMethodInfo")
      .SetInitialValue(() => moduleType.GetMethod(
          configureMethodName,
          BindingFlags.Instance | BindingFlags.Public
        )!
      );

    AddApplyConfigurationClass(fileBuilder, providerConfiguration);
  }

  public void AddApplyConfigurationClass(
    FileBuilder fileBuilder,
    ModuleProviderConfiguration providerConfiguration
  ) {
    var moduleType = providerConfiguration.Declaration.InterfaceDeclaration;
    var providerType = providerConfiguration.ProviderType;
    var configureMethodName = new MumeiStringExpression(providerConfiguration.ConfigurationMethod.Name);

    // file class ApplyIWeatherServiceIHttpClientConfiguration : ApplyBindingConfigurationFactory<IHttpClient> {
    var configurationClass = fileBuilder.AddClassDeclaration($"Applyλ{configureMethodName}", SyntaxVisibility.File);
    var applyBindingConfigurationType = typeof(ApplyBindingConfigurationFactory<>).MakeGenericType(providerType);
    configurationClass.AddBaseClass(applyBindingConfigurationType);

    // private readonly MethodInfo _configureMethod;
    var configureMethodField = configurationClass.AddField<MethodInfo>(
      "_configureMethod",
      SyntaxVisibility.Private | SyntaxVisibility.ReadOnly
    );

    // private readonly Binding<IHttpClient> _provider;
    var providerBindingType = typeof(Binding<>).MakeGenericType(providerType);
    var configurationTargetProviderField = configurationClass.AddField(
      "_provider",
      providerBindingType,
      SyntaxVisibility.Private | SyntaxVisibility.ReadOnly
    );

    // private readonly object _configureMethodTarget;
    var configureMethodTargetField = configurationClass.AddField(
      "_configureMethodTarget",
      moduleType,
      SyntaxVisibility.Private | SyntaxVisibility.ReadOnly
    );

    /*
      public ApplyIWeatherServiceIHttpClientConfiguration(
        MethodInfo configureMethod,
        object configureMethodTarget,
        Binding<IHttpClient> httpClientProvider
      ) {
        _configureMethod = configureMethod;
        _configureMethodTarget = configureMethodTarget;
        _httpClientProvider = httpClientProvider;
      }
      */
    configurationClass.AddConstructor(
      Param.Create<MethodInfo>("configureMethod"),
      Param.Create(moduleType, "configurationMethodTarget"),
      Param.Create(providerBindingType, "configurationTargetProvider"),
      (
        configureMethod,
        configureMethodTarget,
        configurationTargetProvider,
        body
      ) => {
        body.Assign(configureMethodField, configureMethod);
        body.Assign(configureMethodTargetField, configureMethodTarget);
        body.Assign(configurationTargetProviderField, configurationTargetProvider);
      });

    /*
    private IHttpClient ConfigureHttpClient(IHttpClient httpClient) {
      return (IHttpClient)_configureMethod.Invoke(_configureMethodTarget, new object[] { httpClient })!;
    }
    */
    var applyConfigurationMethod = configurationClass
      .AddMethod(
        providerType,
        Param.Create(providerType, "provider"),
        (provider, b) =>
          b.Return(() =>
            configureMethodField.Value.Invoke(configureMethodTargetField.Value, new[] { provider.Value })
          )
      );

    /*
    public override IHttpClient Get(IInjector? scope) {
      return ConfigureHttpClient(_httpClientProvider.Get());
    }
    */
    configurationClass.AddMethod(
      providerType,
      (ParameterSyntax<IInjector?> scope, BlockSyntaxBuilder b) =>
        b.Return(() => applyConfigurationMethod.Invoke(scope.Value))
    );
  }

  public void AddModuleProvider(
    ClassSyntaxBuilder cs,
    ProviderSpecification providerSpec,
    ModuleDeclaration moduleDeclaration
  ) {
    var providerType = typeof(Binding<>).MakeGenericType(providerSpec.ProviderType);
    var field = new FieldSyntax(providerType, providerType.Name, null!);
    field.Visibility = SyntaxVisibility.Internal | SyntaxVisibility.ReadOnly;
  }
}