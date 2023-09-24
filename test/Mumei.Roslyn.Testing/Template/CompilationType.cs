using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mumei.Roslyn.Testing.Template;

// This should ideally be a ref struct but that makes
// it hard to use in tests, which need to be classes.
// The additional memory we allocate for the StringBuilder
// instead of the DefaultInterpolatedStringHandler should not
// be a problem since we are only using this in tests.
[InterpolatedStringHandler]
public struct CompilationType : IEquatable<CompilationType>, ITemplateFormattable {
  private string _name;

  private HashSet<Type> _typeReferences;
  private HashSet<CompilationType> _sourceReferences;
  private StringBuilder _builder;

  public string Name => _name;

  public IEnumerable<CompilationType> ReferencedSources => _sourceReferences.Concat(ImmutableArray.Create(this));

  public IEnumerable<Type> ReferencedTypes => _typeReferences;

  public CompilationType(int literalLength, int formattedCount, [CallerMemberName] string memberName = "") {
    _name = memberName;
    _typeReferences = new HashSet<Type>();
    _sourceReferences = new HashSet<CompilationType>();
    _builder = new StringBuilder(literalLength);
  }

  public static implicit operator TypeSource(CompilationType compilationType) {
    return compilationType.ToSource();
  }

  public void AppendLiteral(string s) {
    _builder.Append(s);
  }

  public void AppendFormatted(string s) {
    _builder.Append(s);
  }

  public void AppendFormatted(Type t, string? format = null) {
    _typeReferences.Add(t);
    AppendFormattable(new CompilationTypeFormattable(t), format);
  }

  public void AppendFormatted(ITemplateFormattable formattable, string? format = null) {
    foreach (var s in formattable.ReferencedSources) {
      _sourceReferences.Add(s);
    }

    foreach (var t in formattable.ReferencedTypes) {
      _typeReferences.Add(t);
    }

    AppendFormattable(formattable, format);
  }

  private void AppendFormattable(IFormattable formattable, string? format = null) {
    _builder.Append(formattable.ToString(format, null));
  }

  public TypeSource ToSource() {
    return new TypeSource {
      Name = _name,
      Source = _builder.ToString(),
      TypeReferences = _typeReferences.ToImmutableArray(),
      SourceReferences = _sourceReferences.ToImmutableArray()
    };
  }

  public string ToString(string? format, IFormatProvider? formatProvider) {
    if (_name.EndsWith("Attribute")) {
      format ??= CompilationTemplateFormat.Attribute;
    }

    return format switch {
      CompilationTemplateFormat.Display => _name,
      CompilationTemplateFormat.Attribute => $"[{_name}]",
      _ => _builder.ToString()
    };
  }

  public bool Equals(CompilationType other) {
    return _name == other._name;
  }

  public override bool Equals(object? obj) {
    return obj is CompilationType other && Equals(other);
  }

  public override int GetHashCode() {
    return _name.GetHashCode();
  }

  public override string ToString() {
    return _name;
  }

  public static bool operator ==(CompilationType left, CompilationType right) {
    return left.Equals(right);
  }

  public static bool operator !=(CompilationType left, CompilationType right) {
    return !(left == right);
  }
}