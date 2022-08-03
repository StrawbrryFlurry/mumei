namespace Mumei.Core;

public class Binding<TProvider> {
  public TProvider Get() {
    return default;
  }

  public static implicit operator TProvider(Binding<TProvider> binding) {
    return binding.Get();
  }
}