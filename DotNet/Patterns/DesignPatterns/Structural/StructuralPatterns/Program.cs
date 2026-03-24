/*
 * Project: StructuralPatterns
 * Description:
 * This sample demonstrates the seven structural patterns in a modern C# console app:
 * Adapter, Bridge, Composite, Decorator, Facade, Flyweight, and Proxy.
 * Each example focuses on how objects collaborate while keeping application structure flexible.
 */

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Structural Patterns Sample");
        Console.WriteLine("==========================");

        RunAdapter();
        RunBridge();
        RunComposite();
        RunDecorator();
        RunFacade();
        RunFlyweight();
        RunProxy();
    }

    // Adapter makes an incompatible service fit a new abstraction.
    private static void RunAdapter()
    {
        PrintSection("Adapter");

        IWeatherClient client = new LegacyWeatherAdapter(new LegacyWeatherService());
        Console.WriteLine(client.GetForecast("Seattle"));
    }

    // Bridge separates an abstraction from its implementation so both can vary independently.
    private static void RunBridge()
    {
        PrintSection("Bridge");

        var digest = new ReleaseDigest(new EmailMessageSender());
        Console.WriteLine(digest.Send("Blue/green swap completed."));
    }

    // Composite treats individual items and groups through one shared abstraction.
    private static void RunComposite()
    {
        PrintSection("Composite");

        var epic = new WorkItemGroup("Payments epic");
        epic.Add(new WorkItemLeaf("Retry failed charges", 5));
        epic.Add(new WorkItemLeaf("Emit dead-letter metrics", 3));

        Console.WriteLine($"{epic.Name} total points: {epic.GetStoryPoints()}");
    }

    // Decorator adds behavior before or after delegating to the wrapped object.
    private static void RunDecorator()
    {
        PrintSection("Decorator");

        IMetricsClient client = new CachedMetricsClient(new LiveMetricsClient());
        Console.WriteLine(client.GetMetric("employee-api-latency"));
        Console.WriteLine(client.GetMetric("employee-api-latency"));
    }

    // Facade offers one simplified entry point over a more complex subsystem.
    private static void RunFacade()
    {
        PrintSection("Facade");

        var facade = new ReleaseFacade(new BuildService(), new TestService(), new DeploymentService());
        Console.WriteLine(facade.Release("EmployeeApi"));
    }

    // Flyweight shares immutable state so many objects can reuse it cheaply.
    private static void RunFlyweight()
    {
        PrintSection("Flyweight");

        var factory = new StatusBadgeStyleFactory();
        var readyStyle = factory.GetStyle("Ready");
        var anotherReadyStyle = factory.GetStyle("Ready");

        Console.WriteLine($"Shared instance reused: {ReferenceEquals(readyStyle, anotherReadyStyle)}");
        Console.WriteLine(readyStyle.Render("Ready for release"));
    }

    // Proxy controls access to another object while preserving the same contract.
    private static void RunProxy()
    {
        PrintSection("Proxy");

        IReportService reportService = new AuthorizedReportProxy(new SensitiveReportService(), hasAccess: true);
        Console.WriteLine(reportService.GetQuarterlyReport());
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));
    }
}

internal interface IWeatherClient
{
    string GetForecast(string city);
}

internal sealed class LegacyWeatherService
{
    public string FetchWeather(string postalCode) => $"Legacy forecast for {postalCode}: Clear and 58F";
}

internal sealed class LegacyWeatherAdapter(LegacyWeatherService legacyService) : IWeatherClient
{
    public string GetForecast(string city)
    {
        var postalCode = city.Equals("Seattle", StringComparison.OrdinalIgnoreCase) ? "98101" : "00000";
        return legacyService.FetchWeather(postalCode);
    }
}

internal interface IMessageSender
{
    string SendMessage(string body);
}

internal sealed class EmailMessageSender : IMessageSender
{
    public string SendMessage(string body) => $"Email sent: {body}";
}

internal abstract class MessageDigest(IMessageSender sender)
{
    public string Send(string content) => sender.SendMessage(Format(content));

    protected abstract string Format(string content);
}

internal sealed class ReleaseDigest(IMessageSender sender) : MessageDigest(sender)
{
    protected override string Format(string content) => $"[Release Digest] {content}";
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

internal interface IMetricsClient
{
    string GetMetric(string metricName);
}

internal sealed class LiveMetricsClient : IMetricsClient
{
    public string GetMetric(string metricName) => $"Fetched live metric '{metricName}' at {DateTime.UtcNow:HH:mm:ss}.";
}

internal sealed class CachedMetricsClient(IMetricsClient innerClient) : IMetricsClient
{
    private readonly Dictionary<string, string> cache = new(StringComparer.OrdinalIgnoreCase);

    public string GetMetric(string metricName)
    {
        if (!cache.TryGetValue(metricName, out var value))
        {
            value = $"{innerClient.GetMetric(metricName)} (cache miss)";
            cache[metricName] = value;
        }
        else
        {
            value = $"{value} (cache hit)";
        }

        return value;
    }
}

internal sealed class BuildService
{
    public string Run(string projectName) => $"Build passed for {projectName}.";
}

internal sealed class TestService
{
    public string Run(string projectName) => $"Tests passed for {projectName}.";
}

internal sealed class DeploymentService
{
    public string Run(string projectName) => $"{projectName} deployed to staging.";
}

internal sealed class ReleaseFacade(BuildService buildService, TestService testService, DeploymentService deploymentService)
{
    public string Release(string projectName)
    {
        return string.Join(
            " ",
            buildService.Run(projectName),
            testService.Run(projectName),
            deploymentService.Run(projectName));
    }
}

internal sealed class StatusBadgeStyleFactory
{
    private readonly Dictionary<string, StatusBadgeStyle> styles = new(StringComparer.OrdinalIgnoreCase);

    public StatusBadgeStyle GetStyle(string status)
    {
        if (!styles.TryGetValue(status, out var style))
        {
            style = new StatusBadgeStyle(status, status.Equals("Ready", StringComparison.OrdinalIgnoreCase) ? "green" : "gray");
            styles[status] = style;
        }

        return style;
    }
}

internal sealed class StatusBadgeStyle(string label, string color)
{
    public string Render(string text) => $"[{label}:{color}] {text}";
}

internal interface IReportService
{
    string GetQuarterlyReport();
}

internal sealed class SensitiveReportService : IReportService
{
    public string GetQuarterlyReport() => "Quarterly report: margin improved by 12%.";
}

internal sealed class AuthorizedReportProxy(IReportService innerService, bool hasAccess) : IReportService
{
    public string GetQuarterlyReport() =>
        hasAccess ? innerService.GetQuarterlyReport() : "Access denied.";
}
