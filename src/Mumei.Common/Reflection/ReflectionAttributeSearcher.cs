using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class ReflectionAttributeSearcher<TMemberInfo>
  where TMemberInfo : MemberInfo {
  private readonly TMemberInfo _member;
  private readonly ReflectionPolicies<TMemberInfo> _policies;

  public ReflectionAttributeSearcher(
    TMemberInfo memberInfo
  ) {
    _member = memberInfo;
    _policies = ReflectionPolicies<TMemberInfo>.Instance;
  }

  public bool IsDefined(Type attributeType, bool inherit) {
    return IsDefinedCore(attributeType, _member, inherit);
  }

  private bool IsDefinedCore(Type attributeType, TMemberInfo memberInfo, bool inherit) {
    var attributes = memberInfo.CustomAttributes;

    foreach (var attribute in attributes) {
      if (AttributesHaveSameType(attribute.AttributeType, attributeType)) {
        return true;
      }
    }

    if (!inherit) {
      return false;
    }

    if (memberInfo.DeclaringType?.BaseType is null) {
      return false;
    }

    var baseMember = GetImplicitlyOverriddenBaseClassMember(memberInfo);

    return baseMember is not null && IsDefinedCore(attributeType, baseMember, true);
  }

  public object[] GetCustomAttributes(Type attributeType, Type declaringType, bool inherit) {
    var attributes = SearchCustomAttributeData(declaringType, attributeType, inherit)
      .Where(x => AttributesHaveSameType(x.AttributeType, attributeType))
      .ToArray();

    var result = new List<object>(attributes.Length);

    foreach (var attribute in attributes) {
      result.Add(ReflectionAttributeFactory.CreateInstance(attribute));
    }

    return result.ToArray();
  }

  public object[] GetCustomAttributes(Type declaringType, bool inherit) {
    var attributes = SearchCustomAttributeData(declaringType, null, inherit);
    var result = new List<object>(attributes.Length);

    foreach (var attribute in attributes) {
      result.Add(ReflectionAttributeFactory.CreateInstance(attribute));
    }

    return result.ToArray();
  }

  private static bool AttributesHaveSameType(Type attributeType, Type typeToMatch) {
    if (typeToMatch.IsGenericTypeDefinition) {
      attributeType = attributeType.GetGenericTypeDefinition();
    }

    return attributeType.IsAssignableTo(typeToMatch);
  }

  private CustomAttributeData[] SearchCustomAttributeData(
    Type declaringType,
    Type? optionalTypeFilter,
    bool inherit,
    TMemberInfo? memberInfo = null,
    List<CustomAttributeData>? result = null
  ) {
    result ??= new List<CustomAttributeData>();
    memberInfo ??= _member;
    var attributes = memberInfo.CustomAttributes;

    foreach (var attribute in attributes) {
      if (optionalTypeFilter is null || AttributesHaveSameType(attribute.AttributeType, optionalTypeFilter)) {
        result.Add(attribute);
      }
    }

    if (!inherit) {
      return result.ToArray();
    }

    if (declaringType.BaseType is null) {
      return result.ToArray();
    }

    var baseMember = GetImplicitlyOverriddenBaseClassMember(memberInfo);

    if (baseMember is null) {
      return result.ToArray();
    }

    return SearchCustomAttributeData(declaringType.BaseType, optionalTypeFilter, true, baseMember, result);
  }

  private TMemberInfo? GetImplicitlyOverriddenBaseClassMember(TMemberInfo memberInfo) {
    return _policies.GetImplicitlyOverriddenBaseClassMember(memberInfo);
  }
}