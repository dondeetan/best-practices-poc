/*
 * Dependency Inversion Principle
 * Description: High-level policies should depend on abstractions, not concrete
 * infrastructure. The release coordinator depends on repository and messenger contracts.
 * Usage frequency: Very common in modern .NET apps through dependency injection,
 * ports/adapters, service abstractions, and testable application services.
 */

internal sealed class DependencyInversionPrinciple
{
    public string Description => "Coordinate releases through repository and messenger abstractions.";

    public string UsageFrequency => "Very common";

    public void Run(IWorkItemRepository repository)
    {
        ConsoleSection.Print("Dependency Inversion Principle");

        var coordinator = new ReleaseCoordinator(repository, new SlackReleaseMessenger());
        Console.WriteLine(coordinator.PublishReadyItems());
    }
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
