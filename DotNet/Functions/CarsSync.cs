using Functions.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

// Single Responsibility Principle (SOLID): this function orchestrates synchronization and delegates API/cache details.
public class CarsSync(ICarsApiClient carsApiClient, ICarsCacheWriter carsCacheWriter, ILogger<CarsSync> logger)
{
    private readonly ICarsApiClient _carsApiClient = carsApiClient;
    private readonly ICarsCacheWriter _carsCacheWriter = carsCacheWriter;
    private readonly ILogger<CarsSync> _logger = logger;

    // NCRONTAB with seconds (Runs Daily Midnight Function): "0 0 0 * * *"
    [Function("CarsSync")]
    public async Task RunAsync([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo timerInfo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CarsSync started at {ts}", DateTimeOffset.UtcNow);

        var cars = await _carsApiClient.GetCarsAsync(cancellationToken);
        _logger.LogInformation("Retrieved {count} cars from API", cars.Count);
        await _carsCacheWriter.WriteAsync(cars, cancellationToken);
    }
}
