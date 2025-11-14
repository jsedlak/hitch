using Hitch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HitchPlugin("Service", "SampleWeather", typeof(Samples.MyService.ForecastPluginProvider))]

namespace Samples.MyService;

internal class ForecastPluginProvider : IPluginProvider
{
    public void Attach(IServiceCollection services, IConfigurationSection configurationSection, string? name = null)
    {
        if (name != null)
        {
            services.AddKeyedSingleton<IWeatherService, WeatherForecastService>(
                name, 
                (sp, _) => new WeatherForecastService(name)
            );
        }
    }
}
