using System.Reflection;
using Mumei.DependencyInjection.Injector.Implementation;

namespace Mumei.DependencyInjection.Injector.Resolution;

public static class InjectorTypeActivator {
  public static object CreateInstance(
    Type type,
    IInjector injector,
    IInjector? scope
  ) {
    var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

    if (ctors.Length == 0) {
      throw new MissingMethodException($"No public constructor found for type {type.FullName}.");
    }

    if (ctors.Length > 1) {
      return CreateInstanceWithBestOverloadMatch(type, ctors, injector, scope);
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
        args[i] = parameter.DefaultValue!;
      }
      else {
        args[i] = injector.Get(parameter.ParameterType, scope);
      }
    }

    return ctor.Invoke(args);
  }

  private static object CreateInstanceWithBestOverloadMatch(
    Type t,
    ConstructorInfo[] ctors,
    IInjector injector,
    IInjector? scope
  ) {
    var orderedCtors = ctors.Select(c => (c, c.GetParameters())).OrderBy(c => c.Item2.Length);

    foreach (var (ctor, parameters) in orderedCtors) {
      var args = new object[parameters.Length];

      var hasNullInjectorException = false;
      foreach (var parameter in parameters) {
        try {
          args[parameter.Position] = injector.Get(parameter.ParameterType, scope);
        }
        catch (NullInjector.NullInjectorException e) {
          if (parameter.IsOptional) {
            args[parameter.Position] = parameter.DefaultValue!;
            continue;
          }

          hasNullInjectorException = true;
          break;
        }
      }

      if (hasNullInjectorException) {
        continue;
      }

      return ctor.Invoke(args);
    }

    throw new MissingMethodException($"Could not find a suitable constructor for type {t.FullName}.");
  }
}