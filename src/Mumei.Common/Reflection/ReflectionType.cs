using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Mumei.Common.Utilities;

namespace Mumei.Common.Reflection;

internal sealed class ReflectionType : Type {
  internal static readonly ConcurrentDictionary<Type, Guid> TypeGuids = new();
  internal static readonly ConcurrentDictionary<string, Type> TypeCache = new();

  private readonly FieldInfo[] _fields;
  private readonly Type[] _interfaces;
  private readonly MethodInfo[] _methods;
  private readonly PropertyInfo[] _properties;
  private readonly TypeAttributes _typeAttributes;
  private readonly ConstructorInfo[] _constructors;

  private ReflectionType(
    string name,
    string @namespace,
    Type? baseType,
    Type[] interfaces,
    Type[] typeArguments,
    bool isGenericType,
    TypeAttributes typeAttributes,
    IReadOnlyList<IMethodInfoFactory> methods,
    IReadOnlyList<IConstructorInfoFactory> constructors,
    IReadOnlyList<IFieldInfoFactory> fields,
    IReadOnlyList<IPropertyInfoFactory> properties,
    Module module
  ) {
    GUID = TypeGuids.AddOrUpdate(
      this,
      _ => Guid.NewGuid(),
      (_, guid) => guid
    );

    Module = module;
    Assembly = module.Assembly;
    Name = name;
    Namespace = @namespace;
    BaseType = baseType;
    FullName = GetFullName(name, @namespace, typeArguments);

    IsGenericType = isGenericType;
    GenericTypeArguments = typeArguments;

    _interfaces = interfaces;
    _typeAttributes = typeAttributes;
    _constructors = CreateConstructors(constructors);
    _methods = CreateMethods(methods);
    _properties = CreateProperties(properties);
    _fields = CreateFields(fields);

    TypeCache.TryAdd(FullName, this);
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

    IsGenericType = type.IsGenericType;
    GenericTypeArguments = type.GenericTypeArguments;
    _typeAttributes = type._typeAttributes;
    _interfaces = type._interfaces;
    _methods = type._methods;
    _properties = type._properties;
    _fields = type._fields;

    TypeCache.TryAdd(FullName, this);
  }

  public override Module Module { get; }
  public override string? Namespace { get; }
  public override string Name { get; }

  public override Assembly Assembly { get; }
  public override string? AssemblyQualifiedName { get; } = null;
  public override Type? BaseType { get; }
  public override string? FullName { get; }
  public override Guid GUID { get; }

  public override Type UnderlyingSystemType
    => throw new NotSupportedException("Cannot get clr type from compile time type");

  public override bool IsConstructedGenericType => IsGenericType && GenericTypeArguments.Length > 0;

  public override bool IsGenericType { get; }

  public override Type[] GenericTypeArguments { get; }

  private static Type Create(
    ReflectionType type,
    Type[] typeArguments
  ) {
    var key = GetFullName(type.Name, type.Namespace, typeArguments);
    return TypeCache.GetOrAdd(
      key,
      _ => new ReflectionType(type, typeArguments)
    );
  }

  public static Type Create(
    string name,
    string @namespace,
    Type? baseType,
    Type[] interfaces,
    Type[] typeArguments,
    bool isGenericType,
    TypeAttributes typeAttributes,
    IMethodInfoFactory[] methods,
    IConstructorInfoFactory[] constructors,
    IFieldInfoFactory[] fields,
    IPropertyInfoFactory[] properties,
    Module module
  ) {
    var key = GetFullName(name, @namespace, typeArguments);
    return TypeCache.GetOrAdd(
      key,
      _ => new ReflectionType(
        name,
        @namespace,
        baseType,
        interfaces,
        typeArguments,
        isGenericType,
        typeAttributes,
        methods,
        constructors,
        fields,
        properties,
        module
      )
    );
  }

  private MethodInfo[] CreateMethods(IReadOnlyList<IMethodInfoFactory> methods) {
    var result = new MethodInfo[methods.Count];

    for (var i = 0; i < methods.Count; i++) {
      var method = methods[i];
      result[i] = method.CreateMethodInfo(this);
    }

    return result;
  }

  private FieldInfo[] CreateFields(IReadOnlyList<IFieldInfoFactory> fields) {
    var result = new FieldInfo[fields.Count];

    for (var i = 0; i < fields.Count; i++) {
      var field = fields[i];
      result[i] = field.CreateFieldInfo(this);
    }

    return result;
  }

  private PropertyInfo[] CreateProperties(IReadOnlyList<IPropertyInfoFactory> properties) {
    var result = new PropertyInfo[properties.Count];

    for (var i = 0; i < properties.Count; i++) {
      var property = properties[i];
      result[i] = property.CreatePropertyInfo(this);
    }

    return result;
  }

  private ConstructorInfo[] CreateConstructors(IReadOnlyList<IConstructorInfoFactory> constructors) {
    if (constructors.Count <= 0) {
      return CreateDefaultConstructor();
    }

    var result = new ConstructorInfo[constructors.Count];

    for (var i = 0; i < constructors.Count; i++) {
      var constructor = constructors[i];
      result[i] = constructor.CreateConstructorInfo(this);
    }

    return result;
  }

  private ConstructorInfo[] CreateDefaultConstructor() {
    return new[] {
      ReflectionConstructorInfo.Create(
        ".ctor",
        MethodAttributes.Public,
        this
      )
    };
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
    return _typeAttributes;
  }

  protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder,
    CallingConventions callConvention,
    Type[] types, ParameterModifier[]? modifiers) {
    throw new NotImplementedException();
  }

  public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) {
    return _constructors;
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
    return GetFields(bindingAttr).FirstOrDefault(x => x.Name == name);
  }

  public override FieldInfo[] GetFields(BindingFlags bindingAttr) {
    return _fields
      .Where(x => FilterMemberByBindingFlags(x, bindingAttr))
      .ToArray();
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
    return _methods
      .Where(x => FilterMemberByBindingFlags(x, bindingAttr))
      .ToArray();
  }

  public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) {
    return _properties
      .Where(x => FilterMemberByBindingFlags(x, bindingAttr))
      .ToArray();
  }

  public override object? InvokeMember(
    string name,
    BindingFlags invokeAttr,
    Binder? binder,
    object? target,
    object?[]? args,
    ParameterModifier[]? modifiers,
    CultureInfo? culture,
    string[]? namedParameters
  ) {
    throw new NotSupportedException();
  }

  public override Type MakeGenericType(params Type[] typeArguments) {
    return Create(
      this,
      typeArguments
    );
  }

  protected override bool IsArrayImpl() {
    return false; // Array types should always be available as a runtime type 
  }

  protected override bool IsByRefImpl() {
    return false;
  }

  protected override bool IsCOMObjectImpl() {
    return false; // Don't support COM objects
  }

  protected override bool IsPointerImpl() {
    return FullName == typeof(nint).FullName || FullName == typeof(nuint).FullName;
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
    ParameterModifier[]? modifier
  ) {
    var candidates = GetProperties(bindingAttr);

    if (!candidates.Any()) {
      return null;
    }

    var comparison = GetBindingFlagsStringComparison(bindingAttr);
    return candidates
      .Where(x => x.Name.Equals(name, comparison))
      .Where(x => returnType is null || x.PropertyType == returnType)
      .FirstOrDefault(
        x => types is null ||
             x.GetIndexParameters()
               .Select(p => p.ParameterType)
               .SequenceEqual(types));
  }

  private StringComparison GetBindingFlagsStringComparison(BindingFlags bindingAttr) {
    return bindingAttr.HasFlag(BindingFlags.IgnoreCase)
      ? StringComparison.OrdinalIgnoreCase
      : StringComparison.Ordinal;
  }

  private bool FilterMemberByBindingFlags(PropertyInfo propertyInfo, BindingFlags bindingFlags) {
    return FilterMemberByBindingFlags(propertyInfo.GetMethod ?? propertyInfo.SetMethod!, bindingFlags);
  }

  private bool FilterMemberByBindingFlags(MethodInfo methodInfo, BindingFlags bindingFlags) {
    return FilterMemberByBindingFlags(methodInfo.IsStatic, methodInfo.IsPublic, methodInfo, bindingFlags);
  }

  private bool FilterMemberByBindingFlags(FieldInfo fieldInfo, BindingFlags bindingFlags) {
    return FilterMemberByBindingFlags(fieldInfo.IsStatic, fieldInfo.IsPublic, fieldInfo, bindingFlags);
  }

  private bool FilterMemberByBindingFlags(bool isStatic, bool isPublic, MemberInfo info, BindingFlags bindingFlags) {
    if (bindingFlags.HasFlag(BindingFlags.Static) && !isStatic) {
      return false;
    }

    if (bindingFlags.HasFlag(BindingFlags.Instance) && isStatic) {
      return false;
    }

    if (bindingFlags.HasFlag(BindingFlags.Public) && !isPublic) {
      return false;
    }

    if (bindingFlags.HasFlag(BindingFlags.NonPublic) && isPublic) {
      return false;
    }

    var isSameInstance = info.DeclaringType == this;
    if (bindingFlags.HasFlag(BindingFlags.DeclaredOnly) && !isSameInstance) {
      return false;
    }

    return true;
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

  public override bool Equals(Type? o) {
    return o?.GUID == GUID;
  }

  public override bool Equals(object? o) {
    return o is ReflectionType other && Equals(other);
  }

  public override int GetHashCode() {
    return GUID.GetHashCode();
  }
}