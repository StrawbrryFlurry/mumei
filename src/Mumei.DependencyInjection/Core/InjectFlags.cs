namespace Mumei.Core;

public enum InjectFlags {
  None = 1 << 0,
  Optional = 1 << 2,
  Self = 1 << 3,
  SkipSelf = 1 << 4,
  Host = 1 << 5,
  Lazy = 1 << 6
}

public class OptionalAttribute : Attribute { }

public class SelfAttribute : Attribute { }

public class SkipSelfAttribute : Attribute { }

public class HostSelfAttribute : Attribute { }