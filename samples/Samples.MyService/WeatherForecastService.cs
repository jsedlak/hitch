namespace Samples.MyService;

internal class WeatherForecastService : IWeatherService
{
    static readonly string[] Conditions = new[]
    {
        "Sunny", "Cloudy", "Rainy", "Windy", "Stormy", "Snowy"
    };

    private readonly string _serviceName;

    public WeatherForecastService(string serviceName)
    {
        _serviceName = serviceName;
    }

    public Forecast GetCurrentWeather(string city)
    {
        // Simulate fetching weather data
        return new Forecast
        {
            TemperatureCelsius = Random.Shared.NextDouble() * 40,
            WeatherCondition = Conditions[Random.Shared.Next(0, Conditions.Length)],
        };
    }

    public string Name => _serviceName;
}