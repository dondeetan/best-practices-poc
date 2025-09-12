using System.Text.Json;
using Functions.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Functions;

public class CarsSync(IHttpClientFactory httpClientFactory, IConnectionMultiplexer redis, ILogger<CarsSync> logger)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly ILogger<CarsSync> _logger = logger;

    // NCRONTAB with seconds (runs at minute 0 every hour): "0 0 * * * *"
    [Function("CarsSync")]
    public async Task RunAsync([TimerTrigger("0 0 * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("CarsSync started at {ts}", DateTimeOffset.UtcNow);

        var client = _httpClientFactory.CreateClient("cars-api");

        // Adjust to your endpoint path (e.g., /api/cars)
        var response = await client.GetAsync("api/cars");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cars API returned {status}", response.StatusCode);
            return;
        }

        var json = await response.Content.ReadAsStringAsync();

        var cars = JsonSerializer.Deserialize<List<Car>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Car>();

        _logger.LogInformation("Retrieved {count} cars from API", cars.Count);

        var db = _redis.GetDatabase();

        // Use a pipeline (IBatch) for throughput
        var batch = db.CreateBatch();

        // Keep an index of all car IDs and per-employee car IDs
        var allCarsKey = "cars:all"; // a Redis set of car IDs

        foreach (var car in cars)
        {
            // Store each car as JSON at key "car:{id}"
            string carKey = $"car:{car.Id}";
            string carJson = JsonSerializer.Serialize(car);

            await batch.StringSetAsync(carKey, carJson);

            // Indexes (Sets): global and per-employee
            await batch.SetAddAsync(allCarsKey, car.Id);
            if (car.EmployeeId != 0)
            {
                string empIndexKey = $"employee:{car.EmployeeId}:cars";
                await batch.SetAddAsync(empIndexKey, car.Id);
            }
        }

        // Optional: also store a full snapshot list (expires in 1 hour)
        string snapshotKey = "cars:snapshot";
        await batch.StringSetAsync(snapshotKey, JsonSerializer.Serialize(cars), TimeSpan.FromHours(1));

        batch.Execute();

        _logger.LogInformation("Pushed {count} cars to Redis", cars.Count);
    }
}