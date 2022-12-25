using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class ConstructorReflectionPolicies : ReflectionPolicies<ConstructorInfo> {
  public override bool ImplicitlyOverrides(ConstructorInfo? baseMember, ConstructorInfo? derivedMember) {
    return false;
  }

  protected override IEnumerable<ConstructorInfo> GetDeclaredMembers(TypeInfo typeInfo) {
    return typeInfo.DeclaredConstructors;
  }

  protected override void GetMemberAttributes(
    ConstructorInfo member,
    out MethodAttributes visibility,
    out bool isStatic,
    out bool isVirtual,
    out bool isNewSlot
  ) {
    var methodAttributes = member.Attributes;
    visibility = methodAttributes & MethodAttributes.MemberAccessMask;
    isStatic = 0 != (methodAttributes & MethodAttributes.Static);
    isVirtual = false;
    isNewSlot = false;
  }
}