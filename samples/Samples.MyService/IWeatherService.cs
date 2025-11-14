namespace Samples.MyService;

public interface IWeatherService
{
    Forecast GetCurrentWeather(string city);

    string Name { get; }
}
