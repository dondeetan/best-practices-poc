/*
 * Strategy
 * Description: Selects an algorithm at runtime behind one shared contract.
 * Usage frequency: Very common for pricing, routing, serialization, authentication,
 * retry behavior, sorting, validation, and cloud provider choices.
 */

internal sealed class StrategyPattern
{
    public string Description => "Execute an incident response plan through a selected strategy.";

    public string UsageFrequency => "Very common";

    public void Run()
    {
        ConsoleSection.Print("Strategy");

        var planner = new IncidentResponsePlanner(new PagingResponseStrategy());
        Console.WriteLine(planner.Execute("Critical outage"));
    }
}

internal interface IResponseStrategy
{
    string Respond(string incidentName);
}

internal sealed class PagingResponseStrategy : IResponseStrategy
{
    public string Respond(string incidentName) => $"Paging on-call team for: {incidentName}";
}

internal sealed class IncidentResponsePlanner(IResponseStrategy strategy)
{
    public string Execute(string incidentName) => strategy.Respond(incidentName);
}
