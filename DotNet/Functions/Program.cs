using System;
using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true)
           .AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var config = ctx.Configuration;

        // HttpClient for the external API
        services.AddHttpClient("cars-api", client =>
        {
            var baseUrl = config["CarsApi:BaseUrl"] ?? throw new InvalidOperationException("CarsApi:BaseUrl not set");
            client.BaseAddress = new Uri(baseUrl);
            var apiKey = config["CarsApi:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }
        });

        // Redis connection (using connection string)
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var cs = config["Redis:ConnectionString"] ?? throw new InvalidOperationException("Redis:ConnectionString not set");
            return ConnectionMultiplexer.Connect(cs);
        });
    })
    .Build();

await host.RunAsync();
