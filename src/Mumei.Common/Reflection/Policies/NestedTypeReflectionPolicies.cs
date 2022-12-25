using System.Reflection;

namespace Mumei.Common.Reflection;

public sealed class NestedTypeReflectionPolicies : ReflectionPolicies<Type> {
  protected override IEnumerable<Type> GetDeclaredMembers(TypeInfo typeInfo) {
    return typeInfo.DeclaredNestedTypes;
  }

  protected override void GetMemberAttributes(
    Type member,
    out MethodAttributes visibility,
    out bool isStatic,
    out bool isVirtual,
    out bool isNewSlot
  ) {
    isStatic = true;
    isVirtual = false;
    isNewSlot = false;

    // Since we never search base types for nested types, we don't need to map every visibility value one to one.
    // We just need to distinguish between "public" and "everything else."
    visibility = member.IsNestedPublic ? MethodAttributes.Public : MethodAttributes.Private;
  }

  public override bool ImplicitlyOverrides(Type? baseMember, Type? derivedMember) {
    return false;
  }
}