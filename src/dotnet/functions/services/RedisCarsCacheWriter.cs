using System.Text.Json;
using Functions.Entities;
using Functions.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Functions.Services;

// Facade pattern: gives the sync function one simple Redis persistence operation over several Redis commands and indexes.
public sealed class RedisCarsCacheWriter : ICarsCacheWriter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RedisCarsCacheWriter> _logger;

    public RedisCarsCacheWriter(IConfiguration configuration, ILogger<RedisCarsCacheWriter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task WriteAsync(IReadOnlyCollection<Car> cars, CancellationToken cancellationToken)
    {
        if (!bool.TryParse(_configuration["UseRedisCache"], out var useRedisCache) || !useRedisCache)
        {
            _logger.LogInformation("Redis Cache Not Used");
            return;
        }

        var connectionString = _configuration["RedisConnectionString"]
            ?? throw new InvalidOperationException("RedisConnectionString not set");

        using var redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        var db = redis.GetDatabase();
        var batch = db.CreateBatch();
        const string allCarsKey = "cars:all";

        foreach (var car in cars)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var carKey = $"car:{car.Id}";
            var carJson = JsonSerializer.Serialize(car);

            _ = batch.StringSetAsync(carKey, carJson);
            _ = batch.SetAddAsync(allCarsKey, car.Id);

            if (car.EmployeeId != 0)
            {
                var employeeIndexKey = $"employee:{car.EmployeeId}:cars";
                _ = batch.SetAddAsync(employeeIndexKey, car.Id);
            }
        }

        _ = batch.StringSetAsync("cars:snapshot", JsonSerializer.Serialize(cars), TimeSpan.FromHours(1));
        batch.Execute();

        _logger.LogInformation("Pushed {Count} cars to Redis", cars.Count);
    }
}
