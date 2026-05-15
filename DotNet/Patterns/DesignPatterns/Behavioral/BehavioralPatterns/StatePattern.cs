/*
 * State
 * Description: Changes behavior by swapping the active state object.
 * Usage frequency: Common for workflows, lifecycle models, protocol handlers,
 * approval states, and UI mode management.
 */

internal sealed class StatePattern
{
    public string Description => "Advance a work item by delegating behavior to state objects.";

    public string UsageFrequency => "Common";

    public void Run()
    {
        ConsoleSection.Print("State");

        var item = new WorkItemContext();
        Console.WriteLine(item.Status);
        item.Advance();
        Console.WriteLine(item.Status);
        item.Advance();
        Console.WriteLine(item.Status);
    }
}

internal sealed class WorkItemContext
{
    private IWorkItemState state = new TodoState();

    public string Status => state.Name;

    public void Advance() => state = state.Advance();
}

internal interface IWorkItemState
{
    string Name { get; }
    IWorkItemState Advance();
}

internal sealed class TodoState : IWorkItemState
{
    public string Name => "To Do";

    public IWorkItemState Advance() => new InProgressState();
}

internal sealed class InProgressState : IWorkItemState
{
    public string Name => "In Progress";

    public IWorkItemState Advance() => new DoneState();
}

internal sealed class DoneState : IWorkItemState
{
    public string Name => "Done";

    public IWorkItemState Advance() => this;
}
