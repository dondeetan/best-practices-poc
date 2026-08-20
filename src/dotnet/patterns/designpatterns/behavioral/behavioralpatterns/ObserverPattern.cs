/*
 * Observer
 * Description: Pushes updates to subscribers when a subject changes.
 * Usage frequency: Very common in events, UI notifications, message buses,
 * reactive streams, logging hooks, and telemetry pipelines.
 */

internal sealed class ObserverPattern
{
    public string Description => "Notify subscribed teams when a build completes.";

    public string UsageFrequency => "Very common";

    public void Run()
    {
        ConsoleSection.Print("Observer");

        var pipeline = new BuildPipeline();
        pipeline.Subscribe(new ConsoleBuildSubscriber("Ops"));
        pipeline.Subscribe(new ConsoleBuildSubscriber("QA"));
        pipeline.Complete("Employee API");
    }
}

internal interface IBuildSubscriber
{
    void Update(string buildName, string status);
}

internal sealed class BuildPipeline
{
    private readonly List<IBuildSubscriber> subscribers = new();

    public void Subscribe(IBuildSubscriber subscriber) => subscribers.Add(subscriber);

    public void Complete(string buildName)
    {
        foreach (var subscriber in subscribers)
        {
            subscriber.Update(buildName, "Succeeded");
        }
    }
}

internal sealed class ConsoleBuildSubscriber(string teamName) : IBuildSubscriber
{
    public void Update(string buildName, string status) =>
        Console.WriteLine($"{teamName} notified: {buildName} -> {status}");
}
