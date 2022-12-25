using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class PropertyReflectionPolicies : ReflectionPolicies<PropertyInfo> {
  protected override IEnumerable<PropertyInfo> GetDeclaredMembers(TypeInfo typeInfo) {
    return typeInfo.DeclaredProperties;
  }

  protected override void GetMemberAttributes(PropertyInfo member, out MethodAttributes visibility, out bool isStatic,
    out bool isVirtual, out bool isNewSlot) {
    var accessorMethod = GetAccessorMethod(member);
    if (accessorMethod == null) {
      // If we got here, this is a inherited PropertyInfo that only had private accessors and is now refusing to give them out
      // because that's what the rules of inherited PropertyInfo's are. Such a PropertyInfo is also considered private and will never be
      // given out of a Type.GetProperty() call. So all we have to do is set its visibility to Private and it will get filtered out.
      // Other values need to be set to satisfy C# but they are meaningless.
      visibility = MethodAttributes.Private;
      isStatic = false;
      isVirtual = false;
      isNewSlot = true;
      return;
    }

    var methodAttributes = accessorMethod.Attributes;
    visibility = methodAttributes & MethodAttributes.MemberAccessMask;
    isStatic = 0 != (methodAttributes & MethodAttributes.Static);
    isVirtual = 0 != (methodAttributes & MethodAttributes.Virtual);
    isNewSlot = 0 != (methodAttributes & MethodAttributes.NewSlot);
  }

  public override bool ImplicitlyOverrides(PropertyInfo? baseMember, PropertyInfo? derivedMember) {
    var baseAccessor = GetAccessorMethod(baseMember!);
    var derivedAccessor = GetAccessorMethod(derivedMember!);
    return ReflectionPolicies<MethodInfo>.Instance.ImplicitlyOverrides(baseAccessor, derivedAccessor);
  }

  private static MethodInfo? GetAccessorMethod(PropertyInfo property) {
    return property.GetMethod ?? property.SetMethod;
  }
}