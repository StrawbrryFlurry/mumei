namespace Mumei.Core;

[Flags]
public enum ProviderScope {
  Singleton = 1 << 0,
  Scoped = 1 << 1,
  Transient = 1 << 2
}