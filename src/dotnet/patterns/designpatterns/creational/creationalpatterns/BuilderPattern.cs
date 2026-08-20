/*
 * Builder
 * Description: Assembles a complex object step by step while keeping construction readable.
 * Usage frequency: Common; often used for immutable options, test data, request payloads,
 * fluent APIs, and objects with many optional settings.
 */

internal sealed class BuilderPattern
{
    public string Description => "Build release notes through a fluent step-by-step API.";

    public string UsageFrequency => "Common";

    public void Run()
    {
        ConsoleSection.Print("Builder");

        var releaseNotes = new ReleaseNotesBuilder()
            .ForVersion("2026.03")
            .AddFeature("Resilient payment retries")
            .AddFeature("Faster employee search")
            .AddRisk("Cache warmup may take two minutes after deploy")
            .Build();

        Console.WriteLine(releaseNotes);
    }
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
