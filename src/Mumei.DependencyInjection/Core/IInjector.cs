namespace Mumei.DependencyInjection.Core;

public interface IInjector {
  /// <summary>
  ///   The injector that was used to create this injector.
  ///   Usually, if this injector is a module, the parent will
  ///   be the module who has imported this module.
  /// </summary>
  public IInjector Parent { get; }

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None);
  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None);
}