var builder = DistributedApplication.CreateBuilder(args);

var hitch = builder.AddHitch(config => config.WithFilePattern("Samples.*.dll"))
    .WithWeather("sample1")
    .WithWeather("sample2");

builder.AddProject<Projects.Samples_TestApi>("samples-testapi")
    .WithReference(hitch);

builder.Build().Run();
