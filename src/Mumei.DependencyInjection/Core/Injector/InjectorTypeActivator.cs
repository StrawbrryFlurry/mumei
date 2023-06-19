using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Mumei.DependencyInjection.Core;

public static class InjectorTypeActivator {
  public static object CreateInstance(Type type, IInjector injector) {
    var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

    if (ctors.Length == 0) {
      throw new MissingMethodException($"No public constructor found for type {type.FullName}.");
    }

    if (ctors.Length > 1) {
      throw new AmbiguousMatchException($"Multiple public constructors found for type {type.FullName}.");
    }

    var ctor = ctors[0];

    var parameters = ctor.GetParameters();

    if (parameters.Length == 0) {
      return ctor.Invoke(Array.Empty<object>());
    }

    var args = new object[parameters.Length];
    for (var i = 0; i < parameters.Length; i++) {
      var parameter = parameters[i];

      if (parameter.IsOptional) {
        args[i] = parameter.DefaultValue;
      }
      else {
        args[i] = injector.Get(parameter.ParameterType);
      }
    }

    return ctor.Invoke(args);
  }
}