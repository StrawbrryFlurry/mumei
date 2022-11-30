using Microsoft.CodeAnalysis;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class ClassSyntaxBuilder<TClass> : TypeSyntax {
  public ClassSyntaxBuilder() : base(typeof(TClass).Name) { }

  public FieldSyntax<TField> AddField<TField>(string name) {
    return new FieldSyntax<TField>(name, this);
  }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }
}

public sealed class ClassSyntaxBuilder : SyntaxWriter {
  private readonly List<MemberFieldInfo> _fields = new();
  private readonly HashSet<string> _interfaces = new();

  private readonly string _name;
  private readonly SyntaxVisibility _visibility;

  private Type? _baseType;

  public ClassSyntaxBuilder(string className, SyntaxVisibility classVisibility = SyntaxVisibility.Internal) {
    _name = className;
    _visibility = classVisibility;
  }

  private bool HasInterfaces => _interfaces.Count > 0;
  private bool HasBaseType => _baseType is not null;
  private bool HasBaseTypes => HasInterfaces || HasBaseType;

  private bool HasFields => _fields.Count > 0;

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

    _interfaces.Add(@interface.FullName);
  }

  public void AddInterfaceImplementation(ITypeSymbol @interface) {
    if (@interface.TypeKind != TypeKind.Interface) {
      throw new ArgumentException($"Type '{@interface}' is not an interface");
    }

    _interfaces.Add(@interface.Name);
  }

  public FieldSyntax AddField(Type type, string name) {
    return null!;
  }

  public FieldSyntax<TField> AddField<TField>(string name, SyntaxVisibility visibility = SyntaxVisibility.Private) {
    _fields.Add(new MemberFieldInfo());
    return null!;
  }

  public FieldSyntax<TFieldHelper> AddField<TFieldHelper>(
    string name,
    Type type,
    SyntaxVisibility visibility = SyntaxVisibility.Private) {
    _fields.Add(new MemberFieldInfo());
    return null!;
  }

  public FieldSyntax AddField(
    string name,
    Type type,
    SyntaxVisibility visibility = SyntaxVisibility.Private) {
    _fields.Add(new MemberFieldInfo());
    return null!;
  }

  public PropertySyntax<TProperty> AddProperty<TProperty>(string name) {
    return new PropertySyntax<TProperty>(name, null!);
  }

  public MethodSyntax AddMethod(Type returnType, Param param1, MethodBuilder<object> body) {
    var parameters = new List<object>();
    foreach (var parameterInfo in body.Method.GetParameters()) {
      var parameter = ParameterSyntax.MakeGenericParameter(parameterInfo.ParameterType, parameterInfo.Name);
      parameters.Add(parameter);
    }

    return null!;
  }

  public MethodSyntax AddMethod<TArg>(Type returnType, MethodBuilder<TArg> body) {
    var parameters = new List<object>();
    foreach (var parameterInfo in body.Method.GetParameters()) {
      var parameter = ParameterSyntax.MakeGenericParameter(parameterInfo.ParameterType, parameterInfo.Name);
      parameters.Add(parameter);
    }

    return null!;
  }

  public BlockSyntaxBuilder AddConstructor() {
    return new BlockSyntaxBuilder();
  }

  public void AddConstructor(BlockBuilder builder) { }

  public void AddConstructor<TArg1, TArg2, TArg3>(MethodBuilder<TArg1, TArg2, TArg3> builder) { }

  public void AddConstructor(
    Param param1,
    Param param2,
    Param param3,
    MethodBuilder<object, object, object> builder) { }

  public override string ToString() {
    var visibility = _visibility.ToVisibilityString();
    WriteLineStart($"{visibility} class {_name} ");

    if (HasBaseTypes) {
      WriteDerivedTypesToClassDefinition();
    }

    WriteLineEnd("{");
    WriteLine("}");
    return ToSyntax();
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

    var interfaceNames = _interfaces;
    Write(string.Join(", ", interfaceNames));
    Write(" ");
  }

  public void AddBaseClass(Type type) {
    throw new NotImplementedException();
  }

  internal class MemberFieldInfo {
    public string Name { get; }
    public Type FieldType { get; }
    public Type[] Attributes { get; }
  }
}