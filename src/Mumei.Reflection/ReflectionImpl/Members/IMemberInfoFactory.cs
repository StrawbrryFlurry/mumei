using System.Reflection;

namespace Mumei.Common;

internal interface IMemberInfoFactory {
  public string MemberInfoName { get; }

  public MemberInfo CreateMemberInfo(Type declaringType);
}