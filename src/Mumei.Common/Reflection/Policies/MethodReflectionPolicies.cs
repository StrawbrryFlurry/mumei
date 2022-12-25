using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class MethodReflectionPolicies : ReflectionPolicies<MethodInfo> {
  public override bool ImplicitlyOverrides(MethodInfo? baseMember, MethodInfo? derivedMember) {
    return AreNamesAndSignaturesEqual(baseMember!, derivedMember!);
  }

  protected override IEnumerable<MethodInfo> GetDeclaredMembers(TypeInfo typeInfo) {
    return typeInfo.DeclaredMethods;
  }

  protected override void GetMemberAttributes(
    MethodInfo member,
    out MethodAttributes visibility,
    out bool isStatic,
    out bool isVirtual,
    out bool isNewSlot
  ) {
    var methodAttributes = member.Attributes;
    visibility = methodAttributes;
    isStatic = (methodAttributes & MethodAttributes.Static) != 0;
    isVirtual = (methodAttributes & MethodAttributes.Virtual) != 0;
    isNewSlot = (methodAttributes & MethodAttributes.NewSlot) != 0;
  }
}