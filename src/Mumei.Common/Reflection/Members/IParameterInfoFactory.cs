using System.Reflection;

namespace Mumei.Common.Reflection;

public interface IParameterInfoFactory {
  public ParameterInfo CreateParameterInfo(Type declaringType);
}