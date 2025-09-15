using System.Net.Http.Headers;
using System.Text.Json;
using Functions.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Functions;

public class CarsSync(IHttpClientFactory httpClientFactory, ILogger<CarsSync> logger, IConfiguration configuration)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<CarsSync> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    // NCRONTAB with seconds (Runs Daily Midnight Function): "0 0 0 * * *"
    [Function("CarsSync")]
    public async Task RunAsync([TimerTrigger("0 0 0 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("CarsSync started at {ts}", DateTimeOffset.UtcNow);

        var clientWithBasicAuth = _httpClientFactory.CreateClient("cars-api");

        // Get Token
        var tokenresponse = await clientWithBasicAuth.PostAsync("auth/token", null);
        if (!tokenresponse.IsSuccessStatusCode)
        {
            _logger.LogError("Cars API returned {status}", tokenresponse.StatusCode);
            return;
        }

        var json = await tokenresponse.Content.ReadAsStringAsync();

        var token = JsonSerializer.Deserialize<Token>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new Token();

        var clientWithTokenAuth = _httpClientFactory.CreateClient();
        clientWithTokenAuth.BaseAddress = clientWithBasicAuth.BaseAddress;
        clientWithTokenAuth.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Adjust to your endpoint path (e.g., /api/cars)
        var response = await clientWithTokenAuth.GetAsync("api/cars");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Cars API returned {status}", response.StatusCode);
            return;
        }

        var jsonresponse = await response.Content.ReadAsStringAsync();

        var cars = JsonSerializer.Deserialize<List<Car>>(jsonresponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Car>();

        _logger.LogInformation("Retrieved {count} cars from API", cars.Count);

        if (bool.Parse(_configuration["UseRedisCache"]))
        {
            var connectionString = _configuration["RedisConnectionString"] ?? throw new InvalidOperationException("RedisConnectionString not set");
            var redis = ConnectionMultiplexer.Connect(connectionString);
            var db = redis.GetDatabase();

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
        else
        { 
            _logger.LogInformation("Redis Cache Not Used");
        }     
    }
}