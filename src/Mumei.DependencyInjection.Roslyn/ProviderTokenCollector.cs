using Mumei.DependencyInjection.Providers.Registration;
using Mumei.Roslyn.Reflection;

namespace Mumei.DependencyInjection.Roslyn;

internal struct ProviderTokenCollector {
  public static object? CollectFromProvideAttribute(RoslynAttribute provideAttribute, DiagnosticReporter reporter) {
    var argument = provideAttribute.GetArgument<object?>(nameof(ProvideAttribute.Token), 0);
    if (argument is null) {
      reporter.ReportNoProviderTokenProvided();
      return null;
    }

    return argument;
  }
}