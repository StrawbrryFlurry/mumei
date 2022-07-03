namespace Mumei.Core; 

public interface IModule {
  public abstract string Providers();
  public abstract string Imports();
  public abstract string Component();
  public abstract string Exports();
}