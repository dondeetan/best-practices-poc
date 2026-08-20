/*
 * Mediator
 * Description: Centralizes collaboration so peers stay loosely coupled.
 * Usage frequency: Common in UI coordination, domain events, request dispatchers,
 * workflow orchestration, and chat or notification flows.
 */

internal sealed class MediatorPattern
{
    public string Description => "Coordinate standup participants through a mediator.";

    public string UsageFrequency => "Common";

    public void Run()
    {
        ConsoleSection.Print("Mediator");

        var mediator = new StandupMediator();
        var developer = new StandupParticipant("Developer", mediator);
        var qa = new StandupParticipant("QA", mediator);

        mediator.Register(developer);
        mediator.Register(qa);

        developer.Send("API changes are ready for regression testing.");
    }
}

internal sealed class StandupMediator
{
    private readonly List<StandupParticipant> participants = new();

    public void Register(StandupParticipant participant) => participants.Add(participant);

    public void Broadcast(string sender, string message)
    {
        foreach (var participant in participants.Where(participant => participant.Name != sender))
        {
            participant.Receive($"{sender}: {message}");
        }
    }
}

internal sealed class StandupParticipant(string name, StandupMediator mediator)
{
    public string Name { get; } = name;

    public void Send(string message) => mediator.Broadcast(Name, message);

    public void Receive(string message) => Console.WriteLine($"{Name} received -> {message}");
}
