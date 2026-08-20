/*
 * Flyweight
 * Description: Shares immutable state so many objects can reuse it cheaply.
 * Usage frequency: Occasional; most useful for high-volume objects, rendering,
 * parsing, caching, and repeated domain metadata.
 */

internal sealed class FlyweightPattern
{
    public string Description => "Reuse immutable status badge styles across repeated labels.";

    public string UsageFrequency => "Occasional";

    public void Run()
    {
        ConsoleSection.Print("Flyweight");

        var factory = new StatusBadgeStyleFactory();
        var readyStyle = factory.GetStyle("Ready");
        var anotherReadyStyle = factory.GetStyle("Ready");

        Console.WriteLine($"Shared instance reused: {ReferenceEquals(readyStyle, anotherReadyStyle)}");
        Console.WriteLine(readyStyle.Render("Ready for release"));
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
