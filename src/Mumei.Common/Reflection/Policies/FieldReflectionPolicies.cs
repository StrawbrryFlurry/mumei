using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class FieldReflectionPolicies : ReflectionPolicies<FieldInfo> {
  protected override IEnumerable<FieldInfo> GetDeclaredMembers(TypeInfo typeInfo) {
    return typeInfo.DeclaredFields;
  }

  protected override void GetMemberAttributes(
    FieldInfo member,
    out MethodAttributes visibility,
    out bool isStatic,
    out bool isVirtual,
    out bool isNewSlot
  ) {
    var fieldAttributes = member.Attributes;
    visibility = (MethodAttributes)(fieldAttributes & FieldAttributes.FieldAccessMask);
    isStatic = 0 != (fieldAttributes & FieldAttributes.Static);
    isVirtual = false;
    isNewSlot = false;
  }

  public override bool ImplicitlyOverrides(FieldInfo? baseMember, FieldInfo? derivedMember) {
    return false;
  }
}