namespace WeatherApplication.Features.Weather.Services;

public sealed class WeatherService : IWeatherService {
  private readonly HttpClient _httpClient;
  private readonly ILogger<WeatherService> _logger;

  public WeatherService(
    ILogger<WeatherService> logger,
    HttpClient httpClient
  ) {
    _logger = logger;
    _httpClient = httpClient;
  }

  public string GetWeather() {
    _logger.LogInformation("Getting weather");
    _logger.LogInformation(_httpClient.BaseAddress!.ToString());
    return "Sunny";
  }
}