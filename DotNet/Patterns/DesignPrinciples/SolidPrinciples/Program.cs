/*
 * Project: SolidPrinciples
 * Description:
 * This console sample demonstrates the SOLID principles with small, modern C# examples.
 * Each section keeps the example practical so the output shows how the principle improves
 * maintainability, extensibility, or testability in a typical .NET codebase.
 */

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("SOLID Principles Sample");
        Console.WriteLine("=======================");

        var repository = new InMemoryWorkItemRepository();
        repository.Add(new WorkItem("Harden API authentication", 5, true));
        repository.Add(new WorkItem("Document retry strategy", 2, false));
        repository.Add(new WorkItem("Refactor cache adapter", 3, true));

        RunSingleResponsibility(repository);
        RunOpenClosed();
        RunLiskovSubstitution();
        RunInterfaceSegregation();
        RunDependencyInversion(repository);
    }

    // Single Responsibility Principle:
    // persistence, summarization, and notification live in different types.
    private static void RunSingleResponsibility(IWorkItemRepository repository)
    {
        PrintSection("Single Responsibility Principle");

        var summaryBuilder = new WorkItemSummaryBuilder();
        var notifier = new ConsoleNotifier();

        var readyItems = repository.GetReadyForQa();
        var summary = summaryBuilder.BuildQaSummary(readyItems);

        notifier.Send(summary);
    }

    // Open/Closed Principle:
    // new compensation rules can be introduced without changing the calculator.
    private static void RunOpenClosed()
    {
        PrintSection("Open/Closed Principle");

        var teammate = new TeamMember("Avery", "Engineering Manager", YearsAtCompany: 4, BaseSalary: 125_000m);
        var rules = new ICompensationRule[]
        {
            new BaseSalaryRule(),
            new TenureBonusRule(),
            new LeadershipBonusRule()
        };

        var calculator = new CompensationCalculator(rules);
        Console.WriteLine($"Total compensation for {teammate.Name}: {calculator.Calculate(teammate):C0}");
    }

    // Liskov Substitution Principle:
    // any alert channel can replace another without surprising the caller.
    private static void RunLiskovSubstitution()
    {
        PrintSection("Liskov Substitution Principle");

        var router = new IncidentRouter();
        var channels = new IAlertChannel[]
        {
            new EmailAlertChannel(),
            new TeamsAlertChannel()
        };

        foreach (var channel in channels)
        {
            Console.WriteLine(router.Route(channel, "Deployment completed successfully."));
        }
    }

    // Interface Segregation Principle:
    // readers and editors depend only on the operations they actually need.
    private static void RunInterfaceSegregation()
    {
        PrintSection("Interface Segregation Principle");

        var board = new SprintBacklogBoard();
        var dashboard = new SprintReviewDashboard();
        var refinement = new BacklogRefinementWorkshop();

        board.Add(new WorkItem("Split payment webhook story", 8, false));
        refinement.MarkReadyForQa(board, "Split payment webhook story");
        dashboard.Print(board);
    }

    // Dependency Inversion Principle:
    // the release coordinator depends on abstractions instead of concrete infrastructure.
    private static void RunDependencyInversion(IWorkItemRepository repository)
    {
        PrintSection("Dependency Inversion Principle");

        var coordinator = new ReleaseCoordinator(repository, new SlackReleaseMessenger());
        Console.WriteLine(coordinator.PublishReadyItems());
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));
    }
}

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

internal sealed record TeamMember(string Name, string Role, int YearsAtCompany, decimal BaseSalary);

internal interface ICompensationRule
{
    decimal Apply(decimal runningTotal, TeamMember member);
}

internal sealed class CompensationCalculator(IEnumerable<ICompensationRule> rules)
{
    private readonly IReadOnlyCollection<ICompensationRule> rules = rules.ToArray();

    public decimal Calculate(TeamMember member)
    {
        var total = 0m;

        foreach (var rule in rules)
        {
            total = rule.Apply(total, member);
        }

        return total;
    }
}

internal sealed class BaseSalaryRule : ICompensationRule
{
    public decimal Apply(decimal runningTotal, TeamMember member) => runningTotal + member.BaseSalary;
}

internal sealed class TenureBonusRule : ICompensationRule
{
    public decimal Apply(decimal runningTotal, TeamMember member) =>
        runningTotal + (member.YearsAtCompany * 1_500m);
}

internal sealed class LeadershipBonusRule : ICompensationRule
{
    public decimal Apply(decimal runningTotal, TeamMember member) =>
        member.Role.Contains("Manager", StringComparison.OrdinalIgnoreCase)
            ? runningTotal + 10_000m
            : runningTotal;
}

internal interface IAlertChannel
{
    string Send(string message);
}

internal sealed class EmailAlertChannel : IAlertChannel
{
    public string Send(string message) => $"Email alert: {message}";
}

internal sealed class TeamsAlertChannel : IAlertChannel
{
    public string Send(string message) => $"Teams alert: {message}";
}

internal sealed class IncidentRouter
{
    public string Route(IAlertChannel channel, string message) => channel.Send(message);
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

internal interface IReleaseMessenger
{
    string Broadcast(string message);
}

internal sealed class SlackReleaseMessenger : IReleaseMessenger
{
    public string Broadcast(string message) => $"Slack message sent: {message}";
}

internal sealed class ReleaseCoordinator(IWorkItemRepository repository, IReleaseMessenger messenger)
{
    public string PublishReadyItems()
    {
        var titles = repository.GetReadyForQa().Select(item => item.Title);
        return messenger.Broadcast($"Ready to release: {string.Join(", ", titles)}");
    }
}
