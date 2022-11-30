namespace Mumei.Common.Reflection;

public sealed class MumeiStringExpression {
  private readonly string _s;

  public MumeiStringExpression(string s) {
    _s = s;
  }

  public static implicit operator string(MumeiStringExpression stringExpression) {
    return stringExpression._s;
  }
}