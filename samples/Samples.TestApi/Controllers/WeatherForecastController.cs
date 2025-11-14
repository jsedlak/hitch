using Microsoft.AspNetCore.Mvc;
using Samples.MyService;

namespace Samples.TestApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public WeatherForecastController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    public IEnumerable<string> Get()
    {
        return _serviceProvider
            .GetKeyedServices<IWeatherService>(KeyedService.AnyKey)
            .Select(m => m.Name);
    }

    [HttpGet("{providerName}/{city}")]
    public Forecast Get([FromRoute]string providerName, [FromRoute]string city)
    {
        var service = _serviceProvider.GetRequiredKeyedService<IWeatherService>(providerName);
        return service.GetCurrentWeather(city);
    }
}
