// ReSharper disable once CheckNamespace

using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxBuilders;

public abstract class TypeSyntax : Syntax {
  private readonly List<MemberSyntax> _members = new();

  public readonly Type[] TypeArguments = Type.EmptyTypes;
  public bool IsGenericType => TypeArguments.Length > 0;

  public IEnumerable<MemberSyntax> Members => _members;
  public bool HasMembers => _members.Count > 0;

  protected TypeSyntax(string identifier) : base(identifier) {
  }

  protected TypeSyntax(string identifier, Syntax? parent) : base(identifier, parent) {
  }

  public void AddMember(MemberSyntax member) {
    _members.Add(member);
  }

  public T AddNewMember<T>(string name, Type type) where T : MemberSyntax {
    var member = (T) Activator.CreateInstance(typeof(T), name, type);
    AddMember(member);
    return member;
  }

  public TBuilder CreateMemberBuilder<TBuilder>(string name, Type type) {
    // There is by design no constraint the builder type because that would
    // make the API really verbose to use. Once source generators support a
    // runtime with covariance on derived types, we can change the builders
    // Build method to return the concrete type and therefore omit the generic.
    var builder = Activator.CreateInstance(typeof(TBuilder), this, name, type);

    return (TBuilder) builder;
  }

  public T? GetMember<T>(string name) where T : MemberSyntax {
    return Members.OfType<T>().SingleOrDefault(m => m.Identifier == name);
  }

  public IEnumerable<T> GetMembers<T>() where T : MemberSyntax {
    return Members.OfType<T>();
  }

  /// <summary>
  ///   Writes all members of the type to the syntax writer.
  /// </summary>
  /// <param name="writer"></param>
  protected void WriteMembers(ITypeAwareSyntaxWriter writer) {
  }
}