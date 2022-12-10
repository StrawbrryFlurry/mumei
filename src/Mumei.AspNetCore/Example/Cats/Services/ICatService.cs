namespace Mumei.AspNetCore.Example.Cats.Services;

public interface ICatService {
  public Task<IEnumerable<Cat>> GetCatsAsync();
}