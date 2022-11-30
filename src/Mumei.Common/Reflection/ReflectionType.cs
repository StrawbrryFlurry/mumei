using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mumei.Common.Reflection.Members;
using Mumei.Common.Utilities;

namespace Mumei.Common.Reflection;

public sealed class ReflectionType : Type {
  internal static readonly ConcurrentDictionary<Type, Guid> TypeGuids = new();
  internal static readonly ConditionalWeakTable<string, Type> TypeCache = new();

  private readonly FieldInfo[] _fields;
  private readonly Type[] _interfaces;
  private readonly MethodInfo[] _methods;
  private readonly PropertyInfo[] _properties;

  private ReflectionType(
    string name,
    string @namespace,
    Type? baseType,
    Type[] interfaces,
    Type[] typeArguments,
    MethodInfoSpec[] methods,
    FieldInfoSpec[] fields,
    PropertyInfoSpec[] properties,
    Module module
  ) {
    Module = module;
    Assembly = module.Assembly;
    Name = name;
    Namespace = @namespace;
    BaseType = baseType;
    FullName = GetFullName(name, @namespace, typeArguments);

    _interfaces = interfaces;
    _methods = CreateMethods(methods);
    _properties = CreateProperties(properties);
    _fields = CreateFields(fields);

    TypeCache.AddOrUpdate(FullName, this);
  }

  private ReflectionType(
    ReflectionType type,
    Type[] typeArguments
  ) {
    Module = type.Module;
    Assembly = type.Assembly;
    Name = type.Name;
    Namespace = type.Namespace;
    BaseType = type.BaseType;
    FullName = GetFullName(type.Name, type.Namespace, typeArguments);

    _interfaces = type._interfaces;
    _methods = type._methods;
    _properties = type._properties;
    _fields = type._fields;

    TypeCache.AddOrUpdate(FullName, this);
  }

  private static Type Create(
    ReflectionType type,
    Type[] typeArguments
  ) {
    return TypeCache.TryGetValue(GetFullName(type.Name, type.Namespace, typeArguments), out var cached) 
      ? cached 
      : new ReflectionType(type, typeArguments);
  }

  internal static Type Create(
    string name,
    string @namespace,
    Type? baseType,
    Type[] interfaces,
    Type[] typeArguments,
    MethodInfoSpec[] methods,
    FieldInfoSpec[] fields,
    PropertyInfoSpec[] properties,
    Module module
  ) {
    if (TypeCache.TryGetValue(GetFullName(name, @namespace, typeArguments), out var type)) {
      return (ReflectionType)type;
    }
    
    return new ReflectionType(
      name,
      @namespace,
      baseType,
      interfaces,
      typeArguments,
      methods,
      fields,
      properties,
      module
    );
  }

  public override Module Module { get; }
  public override string? Namespace { get; }
  public override string Name { get; }

  public override Assembly Assembly { get; }
  public override string? AssemblyQualifiedName { get; }
  public override Type? BaseType { get; }
  public override string? FullName { get; }
  public override Guid GUID => TypeGuids.GetOrAdd(this, _ => new Guid());

  public override Type UnderlyingSystemType
    => throw new NotSupportedException("Cannot get clr type from compile time type");

  private MethodInfo[] CreateMethods(MethodInfoSpec[] methods) {
    var result = new MethodInfo[methods.Length];

    for (var i = 0; i < methods.Length; i++) {
      var method = methods[i];
      result[i] = new ReflectionMethodInfo(method, this);
    }

    return result;
  }

  private FieldInfo[] CreateFields(FieldInfoSpec[] fields) {
    var result = new FieldInfo[fields.Length];

    for (var i = 0; i < fields.Length; i++) {
      var field = fields[i];
      result[i] = new ReflectionFieldInfo(field, this);
    }

    return result;
  }

  private PropertyInfo[] CreateProperties(PropertyInfoSpec[] properties) {
    var result = new PropertyInfo[properties.Length];

    for (var i = 0; i < properties.Length; i++) {
      var property = properties[i];
      result[i] = new ReflectionPropertyInfo(property, this);
    }

    return result;
  }

  private static string GetFullName(string name, string? @namespace, Type[] typeArguments) {
    var nameWithoutTypeArguments = @namespace is null ? name : $"{@namespace}.{name}";

    if (typeArguments.Length <= 0) {
      return nameWithoutTypeArguments;
    }

    var typeArgumentsString = typeArguments.Select(t => t.FullName).JoinBy(",");
    var fullName = $"{nameWithoutTypeArguments}`{typeArguments.Length}[{typeArgumentsString}]";

    return fullName;
  }

  public override object[] GetCustomAttributes(bool inherit) {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit) {
    throw new NotImplementedException();
  }

  protected override TypeAttributes GetAttributeFlagsImpl() {
    throw new NotImplementedException();
  }

  protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder,
    CallingConventions callConvention,
    Type[] types, ParameterModifier[]? modifiers) {
    throw new NotImplementedException();
  }

  public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) {
    throw new NotImplementedException();
  }

  public override Type? GetElementType() {
    throw new NotImplementedException();
  }

  public override EventInfo? GetEvent(string name, BindingFlags bindingAttr) {
    throw new NotImplementedException();
  }

  public override EventInfo[] GetEvents(BindingFlags bindingAttr) {
    throw new NotImplementedException();
  }

  public override FieldInfo? GetField(string name, BindingFlags bindingAttr) {
    throw new NotImplementedException();
  }

  public override FieldInfo[] GetFields(BindingFlags bindingAttr) {
    var fields = new List<FieldInfo>(_fields);

    if ((bindingAttr & BindingFlags.Public) != 0) {
      fields.RemoveAll(x => !x.IsPublic);
    }

    if ((bindingAttr & BindingFlags.NonPublic) != 0) {
      fields.RemoveAll(x => !x.IsPrivate);
    }

    if ((bindingAttr & BindingFlags.Static) != 0) {
      fields.RemoveAll(x => !x.IsStatic);
    }

    if ((bindingAttr & BindingFlags.Instance) != 0) {
      fields.RemoveAll(x => x.IsStatic);
    }

    return fields.ToArray();
  }
  
  public override MemberInfo[] GetMembers(BindingFlags bindingAttr) {
    var members = new List<MemberInfo>();

    members.AddRange(GetMethods(bindingAttr));
    members.AddRange(GetProperties(bindingAttr));
    members.AddRange(GetFields(bindingAttr));

    return members.ToArray();
  }

  protected override MethodInfo? GetMethodImpl(
    string name,
    BindingFlags bindingAttr,
    Binder? binder,
    CallingConventions callConvention,
    Type[]? types,
    ParameterModifier[]? modifiers
  ) {
    throw new NotImplementedException();
  }

  public override MethodInfo[] GetMethods(BindingFlags bindingAttr) {
    var methods = new List<MethodInfo>(_methods);

    if ((bindingAttr & BindingFlags.Public) != 0) {
      methods.RemoveAll(x => !x.IsPublic);
    }

    if ((bindingAttr & BindingFlags.NonPublic) != 0) {
      methods.RemoveAll(x => !x.IsPrivate);
    }

    if ((bindingAttr & BindingFlags.Static) != 0) {
      methods.RemoveAll(x => !x.IsStatic);
    }

    if ((bindingAttr & BindingFlags.Instance) != 0) {
      methods.RemoveAll(x => x.IsStatic);
    }

    return methods.ToArray();
  }

  public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) {
    var properties = new List<PropertyInfo>(_properties);

    if ((bindingAttr & BindingFlags.Public) != 0) {
      properties.RemoveAll(x => !x.GetMethod!.IsPublic);
    }

    if ((bindingAttr & BindingFlags.NonPublic) != 0) {
      properties.RemoveAll(x => !x.GetMethod!.IsPrivate);
    }

    if ((bindingAttr & BindingFlags.Static) != 0) {
      properties.RemoveAll(x => !x.GetMethod!.IsStatic);
    }

    if ((bindingAttr & BindingFlags.Instance) != 0) {
      properties.RemoveAll(x => x.GetMethod!.IsStatic);
    }

    return properties.ToArray();
  }

  public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target,
    object?[]? args,
    ParameterModifier[]? modifiers, CultureInfo? culture, string[]? namedParameters) {
    throw new NotImplementedException();
  }

  public override Type MakeGenericType(params Type[] typeArguments) {
    return Create(
      this,
      typeArguments
    );
  }
  
  protected override bool IsArrayImpl() {
    throw new NotImplementedException();
  }

  protected override bool IsByRefImpl() {
    return !IsValueType;
  }

  protected override bool IsCOMObjectImpl() {
    return false; // Don't support COM objects
  }

  protected override bool IsPointerImpl() {
    return FullName == typeof(IntPtr).FullName || FullName == typeof(UIntPtr).FullName;
  }

  protected override bool IsPrimitiveImpl() {
    return false; // We should already catch all primitives before this instance is created
  }

  protected override PropertyInfo? GetPropertyImpl(
    string name,
    BindingFlags bindingAttr,
    Binder? binder,
    Type? returnType,
    Type[]? types,
    ParameterModifier[]? modifiers) {
    throw new NotImplementedException();
  }

  protected override bool HasElementTypeImpl() {
    return false;
  }

  public override Type? GetNestedType(string name, BindingFlags bindingAttr) {
    throw new NotImplementedException();
  }

  public override Type[] GetNestedTypes(BindingFlags bindingAttr) {
    throw new NotImplementedException();
  }

  public override Type? GetInterface(string name, bool ignoreCase) {
    var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    return _interfaces.FirstOrDefault(i => string.Equals(i.Name, name, comparison));
  }

  public override Type[] GetInterfaces() {
    return _interfaces;
  }
}