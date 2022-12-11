using System.Reflection;

namespace Mumei.Common.Reflection;

internal interface IMemberInfoFactory {
  public MemberInfo CreateMemberInfo(Type declaringType);
}