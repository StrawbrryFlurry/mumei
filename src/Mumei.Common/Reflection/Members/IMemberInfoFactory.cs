using System.Reflection;

namespace Mumei.Common.Reflection;

internal interface IMemberInfoFactory {
  public string MemberInfoName { get; }

  public MemberInfo CreateMemberInfo(Type declaringType);
}