using System.Reflection;

namespace Mumei.Common;

public interface IParameterInfoFactory {
  public ParameterInfo CreateParameterInfo(Type declaringType);
}