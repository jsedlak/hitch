using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aspire.Hosting;

public static class HitchAspireExtensions
{
    public static IHitchResourceBuilder WithWeather(
        this IHitchResourceBuilder builder,
        string name
    )
    {
        builder.WithPlugin("Service", "SampleWeather", name);

        return builder;
    }
}
