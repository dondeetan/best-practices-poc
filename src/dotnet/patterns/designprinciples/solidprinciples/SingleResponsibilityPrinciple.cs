/*
 * Single Responsibility Principle
 * Description: A class should have one reason to change. In this sample,
 * persistence, summarization, and notification live in different types.
 * Usage frequency: Very common in production code; it is a day-to-day design habit
 * for services, controllers, repositories, validators, and background jobs.
 */

internal sealed class SingleResponsibilityPrinciple
{
    public string Description => "Separate persistence, summarization, and notification responsibilities.";

    public string UsageFrequency => "Very common";

    public void Run(IWorkItemRepository repository)
    {
        ConsoleSection.Print("Single Responsibility Principle");

        var summaryBuilder = new WorkItemSummaryBuilder();
        var notifier = new ConsoleNotifier();

        var readyItems = repository.GetReadyForQa();
        var summary = summaryBuilder.BuildQaSummary(readyItems);

        notifier.Send(summary);
    }
}

internal sealed class WorkItemSummaryBuilder
{
    public string BuildQaSummary(IEnumerable<WorkItem> items)
    {
        var itemList = items.ToList();
        var titles = string.Join(", ", itemList.Select(item => item.Title));
        return $"Ready for QA ({itemList.Count}): {titles}";
    }
}

internal sealed class ConsoleNotifier
{
    public void Send(string message) => Console.WriteLine(message);
}
