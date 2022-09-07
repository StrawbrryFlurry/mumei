using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;
using Mumei.Core;

namespace Mumei.CodeGen;

internal interface IWeatherModule { }

public class TestModuleWriter {
  public void WriteModule() {
    var cb = new ClassSyntaxBuilder("WeatherModule", SyntaxVisibility.Public | SyntaxVisibility.Sealed);

    cb.AddInterfaceImplementation<IModule>();
    cb.AddInterfaceImplementation(typeof(IWeatherModule));

    //var configurationField = cb.AddField(/* typeof(MethodInfo), "ConfigureHttpClientMethodInfo"*/);
    // configurationField.SetDefaultValue(() => typeof(IWeatherModule)
    //.GetMethod(
    //  nameof(IWeatherModule.ConfigureHttpClient),
    //  BindingFlags.Instance | BindingFlags.Public
    // )!);

    cb.AddProperty();
    cb.AddProperty();
    cb.AddProperty();

    cb.AddConstructor();

    cb.AddMethod();
  }
}