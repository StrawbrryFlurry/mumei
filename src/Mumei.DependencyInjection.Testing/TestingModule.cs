using System.Linq.Expressions;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Module;

namespace Mumei.DependencyInjection.Testing;

public sealed class TestingModule {
  public static TestingModuleBuilder<TModule> Create<TModule>() where TModule : IInjector {
    return new TestingModuleBuilder<TModule>();
  }

  public sealed class TestingModuleBuilder<TModule> {
    public TModule Build() {
      return default!;
    }

    public TestingModuleBuilder<TModule> Configure<TConfigureChild>(
      Expression<Func<TModule, TConfigureChild>> childSelector,
      Expression<Action<ModuleReplacement<TConfigureChild>>> childConfigurator
    ) {
      return this;
    }

    public sealed class ModuleReplacement<TConfigureChild> {
      public ModuleReplacement<TConfigureChild> Replace<TReplacement>(
        Expression<Func<TConfigureChild, TReplacement>> childSelector,
        TReplacement replacement
      ) {
        return this;
      }
    }
  }
  /*
   * var module = TestingModule.Create<IAppModule>()
  .Configure(
    x => x.ApplicationModule,
    module => module.Replace(m => m.Ordering, TestingModule.Create<IOrderComponent>().Build())
  ).Configure(
    x => x.PresentationModule,
    _ => TestingModule.Create<IPresentationModule>()
  )
  .Configure(
    x => x.PersistenceModule,
    module => module.Replace(x => x.Options, new PersistenceOptions { ConnectionString = "Local" })
  ).Build();
   */
}