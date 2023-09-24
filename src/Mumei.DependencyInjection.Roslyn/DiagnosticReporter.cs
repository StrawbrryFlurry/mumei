using Microsoft.CodeAnalysis;

namespace Mumei.DependencyInjection.Roslyn;

internal ref struct DiagnosticReporter {
  private readonly SourceProductionContext _ctx;

  public DiagnosticReporter(SourceProductionContext ctx) {
    _ctx = ctx;
  }

  public void ReportNoForwardRefImpl() { }

  public void ReportNoProviderTokenProvided() { }
}