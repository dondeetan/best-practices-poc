/*
 * Liskov Substitution Principle
 * Description: Subtypes should be usable anywhere their base abstraction is expected
 * without surprising the caller. Any alert channel can be routed the same way here.
 * Usage frequency: Common; it matters whenever code relies on inheritance,
 * interfaces, mocks, adapters, or replaceable service implementations.
 */

internal sealed class LiskovSubstitutionPrinciple
{
    public string Description => "Route any alert channel through the same abstraction.";

    public string UsageFrequency => "Common";

    public void Run()
    {
        ConsoleSection.Print("Liskov Substitution Principle");

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
