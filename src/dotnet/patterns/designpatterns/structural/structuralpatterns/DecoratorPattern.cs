/*
 * Decorator
 * Description: Adds behavior before or after delegating to an object with the same contract.
 * Usage frequency: Very common in middleware, streams, logging, caching, authorization,
 * retry policies, and telemetry wrappers.
 */

internal sealed class DecoratorPattern
{
    public string Description => "Add caching behavior around a live metrics client.";

    public string UsageFrequency => "Very common";

    public void Run()
    {
        ConsoleSection.Print("Decorator");

        IMetricsClient client = new CachedMetricsClient(new LiveMetricsClient());
        Console.WriteLine(client.GetMetric("employee-api-latency"));
        Console.WriteLine(client.GetMetric("employee-api-latency"));
    }
}

internal interface IMetricsClient
{
    string GetMetric(string metricName);
}

internal sealed class LiveMetricsClient : IMetricsClient
{
    public string GetMetric(string metricName) => $"Fetched live metric '{metricName}' at {DateTime.UtcNow:HH:mm:ss}.";
}

internal sealed class CachedMetricsClient(IMetricsClient innerClient) : IMetricsClient
{
    private readonly Dictionary<string, string> cache = new(StringComparer.OrdinalIgnoreCase);

    public string GetMetric(string metricName)
    {
        if (!cache.TryGetValue(metricName, out var value))
        {
            value = $"{innerClient.GetMetric(metricName)} (cache miss)";
            cache[metricName] = value;
        }
        else
        {
            value = $"{value} (cache hit)";
        }

        return value;
    }
}
