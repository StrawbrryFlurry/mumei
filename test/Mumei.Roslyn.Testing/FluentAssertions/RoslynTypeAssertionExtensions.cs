using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Reflection;
using Xunit.Sdk;

namespace Mumei.Roslyn.Testing.FluentAssertions;

public static class RoslynTypeAssertionExtensions {
  public static RoslynTypeAssertions Should(this RoslynType subject) {
    return new RoslynTypeAssertions(subject);
  }
}

public sealed class RoslynTypeAssertions
  : ReferenceTypeAssertions<RoslynType, RoslynTypeAssertions> {
  private readonly RoslynType _subject;

  protected override string Identifier { get; } = "RoslynTypeAssertions";

  public RoslynTypeAssertions(RoslynType subject) : base(subject) {
    _subject = subject;
  }

  public AndConstraint<RoslynTypeAssertions> BeSymbol(ITypeSymbol symbol) {
    if (_subject == symbol) {
      return new AndConstraint<RoslynTypeAssertions>(this);
    }

    throw new XunitException($"Expected: {symbol}\nActual: {_subject}");
  }
}