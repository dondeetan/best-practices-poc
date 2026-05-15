/*
 * Prototype
 * Description: Clones an existing object when copying is cheaper or safer than rebuilding
 * it from scratch.
 * Usage frequency: Occasional; useful for templates, simulations, editor state,
 * infrastructure blueprints, and test baselines.
 */

internal sealed class PrototypePattern
{
    public string Description => "Clone an environment blueprint and customize the copy.";

    public string UsageFrequency => "Occasional";

    public void Run()
    {
        ConsoleSection.Print("Prototype");

        var baseline = new EnvironmentBlueprint("Production")
        {
            Modules = new List<string> { "API", "Functions", "Redis" }
        };

        var stagingClone = baseline.Clone("Staging");
        stagingClone.Modules.Add("FeatureFlags");

        Console.WriteLine($"Baseline modules: {string.Join(", ", baseline.Modules)}");
        Console.WriteLine($"Clone modules: {string.Join(", ", stagingClone.Modules)}");
    }
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
