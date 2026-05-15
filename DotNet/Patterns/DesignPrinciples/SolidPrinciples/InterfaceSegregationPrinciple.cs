/*
 * Interface Segregation Principle
 * Description: Clients should not depend on members they do not use.
 * Readers and editors depend only on the backlog operations they need.
 * Usage frequency: Very common in service contracts, SDK wrappers, repositories,
 * UI view models, and test doubles where small interfaces reduce coupling.
 */

internal sealed class InterfaceSegregationPrinciple
{
    public string Description => "Split backlog read and edit operations into focused interfaces.";

    public string UsageFrequency => "Very common";

    public void Run()
    {
        ConsoleSection.Print("Interface Segregation Principle");

        var board = new SprintBacklogBoard();
        var dashboard = new SprintReviewDashboard();
        var refinement = new BacklogRefinementWorkshop();

        board.Add(new WorkItem("Split payment webhook story", 8, false));
        refinement.MarkReadyForQa(board, "Split payment webhook story");
        dashboard.Print(board);
    }
}

internal interface IBacklogReader
{
    IReadOnlyCollection<WorkItem> GetPlannedWork();
}

internal interface IBacklogEditor
{
    void Add(WorkItem item);
    void MarkReadyForQa(string title);
}

internal sealed class SprintBacklogBoard : IBacklogReader, IBacklogEditor
{
    private readonly List<WorkItem> items = new();

    public void Add(WorkItem item) => items.Add(item);

    public IReadOnlyCollection<WorkItem> GetPlannedWork() => items.AsReadOnly();

    public void MarkReadyForQa(string title)
    {
        var index = items.FindIndex(item => item.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
        {
            var item = items[index];
            items[index] = item with { IsReadyForQa = true };
        }
    }
}

internal sealed class SprintReviewDashboard
{
    public void Print(IBacklogReader reader)
    {
        foreach (var item in reader.GetPlannedWork())
        {
            var status = item.IsReadyForQa ? "Ready for QA" : "Still refining";
            Console.WriteLine($"{item.Title} -> {status}");
        }
    }
}

internal sealed class BacklogRefinementWorkshop
{
    public void MarkReadyForQa(IBacklogEditor editor, string title) => editor.MarkReadyForQa(title);
}
