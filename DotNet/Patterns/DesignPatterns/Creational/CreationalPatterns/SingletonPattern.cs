/*
 * Singleton
 * Description: Provides one shared instance for application-wide access to a resource.
 * Usage frequency: Occasional in application code; common inside frameworks. Use carefully
 * because dependency injection is often more testable for shared services.
 */

internal sealed class SingletonPattern
{
    public string Description => "Expose one shared deployment settings catalog.";

    public string UsageFrequency => "Occasional";

    public void Run()
    {
        ConsoleSection.Print("Singleton");

        var catalog = DeploymentSettingsCatalog.Instance;
        catalog.Set("Region", "westus2");
        catalog.Set("BlueGreenEnabled", "true");

        Console.WriteLine($"Singleton values: Region={catalog.Get("Region")}, BlueGreenEnabled={catalog.Get("BlueGreenEnabled")}");
    }
}

internal sealed class DeploymentSettingsCatalog
{
    private static readonly Lazy<DeploymentSettingsCatalog> lazyInstance =
        new(() => new DeploymentSettingsCatalog());

    private readonly Dictionary<string, string> settings = new(StringComparer.OrdinalIgnoreCase);

    private DeploymentSettingsCatalog()
    {
    }

    public static DeploymentSettingsCatalog Instance => lazyInstance.Value;

    public string Get(string key) => settings[key];

    public void Set(string key, string value) => settings[key] = value;
}
