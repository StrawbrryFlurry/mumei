using Mumei.CodeGen.SyntaxWriters;
using static Mumei.CodeGen.SyntaxWriters.TypeDeclarationVisibility;

namespace Mumei.CodeGen.SyntaxNodes;

public class ClassSyntaxBuilder : SyntaxWriter {
  private readonly List<MemberFieldInfo> _fields = new();
  private readonly HashSet<Type> _interfaces = new();

  private readonly string _name;
  private readonly TypeDeclarationVisibility _visibility;

  private Type? _baseType;

  private bool HasInterfaces => _interfaces.Count > 0;
  private bool HasBaseType => _baseType is not null;
  private bool HasBaseTypes => HasInterfaces || HasBaseType;

  private bool HasFields => _fields.Count > 0;

  public ClassSyntaxBuilder(string className, TypeDeclarationVisibility classVisibility = Internal) {
    _name = className;
    _visibility = classVisibility;
  }

  public void DefineBaseType(Type baseType) {
    if (HasBaseType) {
      throw new Exception("Classes can only derive from one type.");
    }

    _baseType = baseType;
  }

  public void DefineBaseType<TBase>() where TBase : class {
    DefineBaseType(typeof(TBase));
  }

  public void AddInterfaceImplementation<TInterface>() where TInterface : class {
    AddInterfaceImplementation(typeof(TInterface));
  }

  public void AddInterfaceImplementation(Type @interface) {
    if (!@interface.IsInterface) {
      throw new ArgumentException($"Type '{@interface}' is not an interface");
    }

    _interfaces.Add(@interface);
  }

  public void AddField() {
  }

  public void AddField<TField>() {
    _fields.Add(new MemberFieldInfo());
  }

  public void AddProperty() {
  }

  public void AddMethod() {
  }

  public void AddConstructor() {
  }

  public override string ToString() {
    var visibility = _visibility.ToVisibilityString();
    WriteLineStart($"{visibility} class {_name} ");

    if (HasBaseTypes) {
      WriteDerivedTypesToClassDefinition();
    }

    WriteLineEnd("{");
    WriteLine("}");
    return base.ToSyntax();
  }

  private void WriteDerivedTypesToClassDefinition() {
    Write(": ");

    if (HasBaseType) {
      Write(_baseType!.FullName!);

      if (HasInterfaces) {
        Write(",");
      }

      Write(" ");
    }

    if (!HasInterfaces) {
      return;
    }

    var interfaceNames = _interfaces.Select(i => i.FullName);
    Write(string.Join(", ", interfaceNames));
    Write(" ");
  }

  internal class MemberFieldInfo {
    public string Name { get; }
    public Type FieldType { get; }
    public Type[] Attributes { get; }
  }
}