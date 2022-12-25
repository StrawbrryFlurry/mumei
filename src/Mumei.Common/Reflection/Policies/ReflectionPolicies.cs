using System.Reflection;

namespace Mumei.Common.Reflection;

/// <summary>
///   Utility class for reflection.
///   See CoreLib source code for more details.
///   https://github.com/dotnet/runtime/blob/main/src/libraries/System.Reflection.MetadataLoadContext/src/System/Reflection/Runtime/BindingFlagSupport/MemberPolicies.cs
/// </summary>
public abstract class ReflectionPolicies<TMemberInfo> where TMemberInfo : MemberInfo {
  static ReflectionPolicies() {
    var t = typeof(TMemberInfo);

    Instance = (t switch {
      _ when t == typeof(MethodInfo) => new MethodReflectionPolicies() as ReflectionPolicies<TMemberInfo>,
      _ when t == typeof(ConstructorInfo) => new ConstructorReflectionPolicies() as ReflectionPolicies<TMemberInfo>,
      _ when t == typeof(FieldInfo) => new FieldReflectionPolicies() as ReflectionPolicies<TMemberInfo>,
      _ when t == typeof(PropertyInfo) => new PropertyReflectionPolicies() as ReflectionPolicies<TMemberInfo>,
      _ when t == typeof(Type) => new NestedTypeReflectionPolicies() as ReflectionPolicies<TMemberInfo>,
      _ => throw new NotSupportedException()
    })!;
  }

  public static ReflectionPolicies<TMemberInfo> Instance { get; }

  public abstract bool ImplicitlyOverrides(TMemberInfo? baseMember, TMemberInfo? derivedMember);

  protected abstract IEnumerable<TMemberInfo> GetDeclaredMembers(
    TypeInfo typeInfo
  );

  /// <summary>
  ///   Returns the
  /// </summary>
  /// <param name="member"></param>
  /// <returns></returns>
  public TMemberInfo? GetImplicitlyOverriddenBaseClassMember(
    TMemberInfo member
  ) {
    var name = member.Name;
    var typeInfo = member.DeclaringType?.GetTypeInfo();
    for (;;) {
      var baseType = typeInfo?.BaseType;
      if (baseType is null) {
        return null;
      }

      typeInfo = baseType.GetTypeInfo();
      foreach (var candidate in GetDeclaredMembers(typeInfo)) {
        if (candidate.Name != name) {
          continue;
        }

        GetMemberAttributes(member, out _, out _, out var isCandidateVirtual, out _);
        if (!isCandidateVirtual) {
          continue;
        }

        if (!ImplicitlyOverrides(candidate, member)) {
          continue;
        }

        return candidate;
      }
    }
  }

  protected abstract void GetMemberAttributes(
    TMemberInfo member,
    out MethodAttributes visibility,
    out bool isStatic,
    out bool isVirtual,
    out bool isNewSlot
  );

  protected static bool AreNamesAndSignaturesEqual(MethodInfo? method1, MethodInfo? method2) {
    if (method1 is null || method2 is null) {
      return false;
    }

    if (method1.Name != method2.Name) {
      return false;
    }

    var p1 = method1.GetParameters();
    var p2 = method2.GetParameters();
    if (p1.Length != p2.Length) {
      return false;
    }

    var isGenericMethod1 = method1.IsGenericMethodDefinition;
    var isGenericMethod2 = method2.IsGenericMethodDefinition;
    if (isGenericMethod1 != isGenericMethod2) {
      return false;
    }

    if (!isGenericMethod1) {
      for (var i = 0; i < p1.Length; i++) {
        var parameterType1 = p1[i].ParameterType;
        var parameterType2 = p2[i].ParameterType;
        if (!parameterType1.Equals(parameterType2)) {
          return false;
        }
      }
    }
    else {
      if (method1.GetGenericArguments().Length != method2.GetGenericArguments().Length) {
        return false;
      }

      for (var i = 0; i < p1.Length; i++) {
        var parameterType1 = p1[i].ParameterType;
        var parameterType2 = p2[i].ParameterType;
        if (!GenericMethodAwareAreParameterTypesEqual(parameterType1, parameterType2)) {
          return false;
        }
      }
    }

    return true;
  }

  private static bool GenericMethodAwareAreParameterTypesEqual(Type t1, Type t2) {
    if (t1 == t2) {
      return true;
    }

    if (!(t1.ContainsGenericParameters && t2.ContainsGenericParameters)) {
      return false;
    }

    if ((t1.IsArray && t2.IsArray) || (t1.IsByRef && t2.IsByRef) || (t1.IsPointer && t2.IsPointer)) {
      if (t1.IsSZArray != t2.IsSZArray) {
        return false;
      }

      if (t1.IsArray && t1.GetArrayRank() != t2.GetArrayRank()) {
        return false;
      }

      return GenericMethodAwareAreParameterTypesEqual(t1.GetElementType()!, t2.GetElementType()!);
    }

    if (t1.IsConstructedGenericType && t2.IsConstructedGenericType) {
      if (t1.GetGenericTypeDefinition() != t2.GetGenericTypeDefinition()) {
        return false;
      }

      var ga1 = t1.GenericTypeArguments;
      var ga2 = t2.GenericTypeArguments;
      if (ga1.Length != ga2.Length) {
        return false;
      }

      for (var i = 0; i < ga1.Length; i++) {
        if (!GenericMethodAwareAreParameterTypesEqual(ga1[i], ga2[i])) {
          return false;
        }
      }

      return true;
    }

    if (t1.IsGenericMethodParameter && t2.IsGenericMethodParameter) {
      return t1.GenericParameterPosition == t2.GenericParameterPosition;
    }

    return false;
  }
}