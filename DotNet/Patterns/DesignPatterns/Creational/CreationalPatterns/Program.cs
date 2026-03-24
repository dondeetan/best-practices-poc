/*
 * Project: CreationalPatterns
 * Description:
 * This sample demonstrates the five classic creational patterns in a modern .NET console app:
 * Abstract Factory, Builder, Factory Method, Prototype, and Singleton.
 * These patterns still underpin many current application architectures and framework designs.
 */

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Creational Patterns Sample");
        Console.WriteLine("==========================");

        RunAbstractFactory();
        RunBuilder();
        RunFactoryMethod();
        RunPrototype();
        RunSingleton();
    }

    // Abstract Factory creates families of related objects without exposing concrete types.
    private static void RunAbstractFactory()
    {
        PrintSection("Abstract Factory");

        ICloudProvisioningFactory factory = new AzureProvisioningFactory();
        var queue = factory.CreateQueueClient();
        var storage = factory.CreateStorageClient();

        Console.WriteLine(queue.Describe());
        Console.WriteLine(storage.Describe());
    }

    // Builder assembles a complex object step by step and keeps construction readable.
    private static void RunBuilder()
    {
        PrintSection("Builder");

        var releaseNotes = new ReleaseNotesBuilder()
            .ForVersion("2026.03")
            .AddFeature("Resilient payment retries")
            .AddFeature("Faster employee search")
            .AddRisk("Cache warmup may take two minutes after deploy")
            .Build();

        Console.WriteLine(releaseNotes);
    }

    // Factory Method lets derived classes decide which concrete product to create.
    private static void RunFactoryMethod()
    {
        PrintSection("Factory Method");

        DocumentExporter exporter = new MarkdownExporter();
        Console.WriteLine(exporter.Export("Sprint summary"));
    }

    // Prototype clones an existing object when copying is cheaper than rebuilding from scratch.
    private static void RunPrototype()
    {
        PrintSection("Prototype");

        var baseline = new EnvironmentBlueprint("Production")
        {
            Modules = new List<string> { "API", "Functions", "Redis" }
        };

        var stagingClone = baseline.Clone("Staging");
        stagingClone.Modules.Add("FeatureFlags");

        Console.WriteLine($"Baseline modules: {string.Join(", ", baseline.Modules)}");
        Console.WriteLine($"Clone modules: {string.Join(", ", stagingClone.Modules)}");
    }

    // Singleton provides one shared instance for application-wide configuration access.
    private static void RunSingleton()
    {
        PrintSection("Singleton");

        var catalog = DeploymentSettingsCatalog.Instance;
        catalog.Set("Region", "westus2");
        catalog.Set("BlueGreenEnabled", "true");

        Console.WriteLine($"Singleton values: Region={catalog.Get("Region")}, BlueGreenEnabled={catalog.Get("BlueGreenEnabled")}");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));
    }
}

internal interface ICloudProvisioningFactory
{
    IQueueClient CreateQueueClient();
    IStorageClient CreateStorageClient();
}

internal interface IQueueClient
{
    string Describe();
}

internal interface IStorageClient
{
    string Describe();
}

internal sealed class AzureProvisioningFactory : ICloudProvisioningFactory
{
    public IQueueClient CreateQueueClient() => new AzureQueueClient();

    public IStorageClient CreateStorageClient() => new AzureBlobStorageClient();
}

internal sealed class AzureQueueClient : IQueueClient
{
    public string Describe() => "Azure Queue client provisioned for asynchronous work.";
}

internal sealed class AzureBlobStorageClient : IStorageClient
{
    public string Describe() => "Azure Blob Storage client provisioned for file retention.";
}

internal sealed class ReleaseNotesBuilder
{
    private readonly List<string> features = new();
    private readonly List<string> risks = new();
    private string version = "unversioned";

    public ReleaseNotesBuilder ForVersion(string value)
    {
        version = value;
        return this;
    }

    public ReleaseNotesBuilder AddFeature(string feature)
    {
        features.Add(feature);
        return this;
    }

    public ReleaseNotesBuilder AddRisk(string risk)
    {
        risks.Add(risk);
        return this;
    }

    public string Build()
    {
        return
            $"Release {version}{Environment.NewLine}" +
            $"Features: {string.Join("; ", features)}{Environment.NewLine}" +
            $"Risks: {string.Join("; ", risks)}";
    }
}

internal abstract class DocumentExporter
{
    public string Export(string content)
    {
        var formatter = CreateFormatter();
        return formatter.Format(content);
    }

    protected abstract IContentFormatter CreateFormatter();
}

internal interface IContentFormatter
{
    string Format(string content);
}

internal sealed class MarkdownExporter : DocumentExporter
{
    protected override IContentFormatter CreateFormatter() => new MarkdownFormatter();
}

internal sealed class MarkdownFormatter : IContentFormatter
{
    public string Format(string content) => $"# {content}{Environment.NewLine}- Generated with MarkdownFormatter";
}

internal sealed class EnvironmentBlueprint(string environmentName)
{
    public string EnvironmentName { get; private set; } = environmentName;
    public List<string> Modules { get; init; } = new();

    public EnvironmentBlueprint Clone(string cloneName)
    {
        return new EnvironmentBlueprint(cloneName)
        {
            Modules = new List<string>(Modules)
        };
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
