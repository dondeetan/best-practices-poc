internal sealed record WorkItem(string Title, int StoryPoints, bool IsReadyForQa);

internal interface IWorkItemRepository
{
    void Add(WorkItem item);
    IReadOnlyCollection<WorkItem> GetAll();
    IReadOnlyCollection<WorkItem> GetReadyForQa();
}

internal sealed class InMemoryWorkItemRepository : IWorkItemRepository
{
    private readonly List<WorkItem> items = new();

    public void Add(WorkItem item) => items.Add(item);

    public IReadOnlyCollection<WorkItem> GetAll() => items.AsReadOnly();

    public IReadOnlyCollection<WorkItem> GetReadyForQa() =>
        items.Where(item => item.IsReadyForQa).ToArray();
}
