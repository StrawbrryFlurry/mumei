namespace Mumei.AspNetCore.Example.Cats.Services;

public class CatService : ICatService {
  public Task<IEnumerable<Cat>> GetCatsAsync() {
    throw new NotImplementedException();
  }
}