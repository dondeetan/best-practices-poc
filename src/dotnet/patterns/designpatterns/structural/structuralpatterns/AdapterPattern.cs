/*
 * Adapter
 * Description: Makes an incompatible service fit the abstraction expected by current code.
 * Usage frequency: Very common when wrapping legacy systems, third-party SDKs,
 * cloud provider clients, and test doubles.
 */

internal sealed class AdapterPattern
{
    public string Description => "Adapt a legacy weather service to a modern weather client contract.";

    public string UsageFrequency => "Very common";

    public void Run()
    {
        ConsoleSection.Print("Adapter");

        IWeatherClient client = new LegacyWeatherAdapter(new LegacyWeatherService());
        Console.WriteLine(client.GetForecast("Seattle"));
    }
}

internal interface IWeatherClient
{
    string GetForecast(string city);
}

internal sealed class LegacyWeatherService
{
    public string FetchWeather(string postalCode) => $"Legacy forecast for {postalCode}: Clear and 58F";
}

internal sealed class LegacyWeatherAdapter(LegacyWeatherService legacyService) : IWeatherClient
{
    public string GetForecast(string city)
    {
        var postalCode = city.Equals("Seattle", StringComparison.OrdinalIgnoreCase) ? "98101" : "00000";
        return legacyService.FetchWeather(postalCode);
    }
}
