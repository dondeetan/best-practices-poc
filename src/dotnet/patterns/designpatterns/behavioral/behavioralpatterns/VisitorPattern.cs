/*
 * Visitor
 * Description: Applies new operations across an object structure without changing element classes.
 * Usage frequency: Occasional; useful for compilers, analyzers, object graphs,
 * report generation, and operations over stable hierarchies.
 */

internal sealed class VisitorPattern
{
    public string Description => "Summarize architecture components through a visitor.";

    public string UsageFrequency => "Occasional";

    public void Run()
    {
        ConsoleSection.Print("Visitor");

        var components = new IArchitectureComponent[]
        {
            new ApiComponent("Employee API"),
            new FunctionComponent("Cars Sync Function")
        };

        var visitor = new ArchitectureSummaryVisitor();
        foreach (var component in components)
        {
            component.Accept(visitor);
        }

        Console.WriteLine(visitor.GetSummary());
    }
}

internal interface IArchitectureComponent
{
    void Accept(IArchitectureVisitor visitor);
}

internal sealed class ApiComponent(string name) : IArchitectureComponent
{
    public string Name { get; } = name;

    public void Accept(IArchitectureVisitor visitor) => visitor.Visit(this);
}

internal sealed class FunctionComponent(string name) : IArchitectureComponent
{
    public string Name { get; } = name;

    public void Accept(IArchitectureVisitor visitor) => visitor.Visit(this);
}

internal interface IArchitectureVisitor
{
    void Visit(ApiComponent component);
    void Visit(FunctionComponent component);
}

internal sealed class ArchitectureSummaryVisitor : IArchitectureVisitor
{
    private readonly List<string> lines = new();

    public void Visit(ApiComponent component) => lines.Add($"API component reviewed: {component.Name}");

    public void Visit(FunctionComponent component) => lines.Add($"Function component reviewed: {component.Name}");

    public string GetSummary() => string.Join(" | ", lines);
}
