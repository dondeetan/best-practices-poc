/*
 * Composite
 * Description: Treats individual objects and groups through the same abstraction.
 * Usage frequency: Common in trees, menus, UI components, documents, rules, and
 * hierarchical work planning.
 */

internal sealed class CompositePattern
{
    public string Description => "Calculate story points across a work-item tree.";

    public string UsageFrequency => "Common";

    public void Run()
    {
        ConsoleSection.Print("Composite");

        var epic = new WorkItemGroup("Payments epic");
        epic.Add(new WorkItemLeaf("Retry failed charges", 5));
        epic.Add(new WorkItemLeaf("Emit dead-letter metrics", 3));

        Console.WriteLine($"{epic.Name} total points: {epic.GetStoryPoints()}");
    }
}

internal interface IWorkItemComponent
{
    string Name { get; }
    int GetStoryPoints();
}

internal sealed class WorkItemLeaf(string name, int storyPoints) : IWorkItemComponent
{
    public string Name { get; } = name;

    public int GetStoryPoints() => storyPoints;
}

internal sealed class WorkItemGroup(string name) : IWorkItemComponent
{
    private readonly List<IWorkItemComponent> children = new();

    public string Name { get; } = name;

    public void Add(IWorkItemComponent component) => children.Add(component);

    public int GetStoryPoints() => children.Sum(child => child.GetStoryPoints());
}
