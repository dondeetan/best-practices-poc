/*
 * Command
 * Description: Encapsulates a request so it can be queued, logged, retried, or undone.
 * Usage frequency: Very common in UI actions, background jobs, workflow engines,
 * message handlers, and deployment automation.
 */

internal sealed class CommandPattern
{
    public string Description => "Queue deployment commands and execute them later.";

    public string UsageFrequency => "Very common";

    public void Run()
    {
        ConsoleSection.Print("Command");

        var invoker = new DeploymentQueue();
        invoker.Enqueue(new DeployEnvironmentCommand("staging"));
        invoker.Enqueue(new DeployEnvironmentCommand("production"));

        foreach (var result in invoker.RunAll())
        {
            Console.WriteLine(result);
        }
    }
}

internal interface ICommand
{
    string Execute();
}

internal sealed class DeployEnvironmentCommand(string environmentName) : ICommand
{
    public string Execute() => $"Deployment command executed for {environmentName}.";
}

internal sealed class DeploymentQueue
{
    private readonly Queue<ICommand> commands = new();

    public void Enqueue(ICommand command) => commands.Enqueue(command);

    public IReadOnlyCollection<string> RunAll()
    {
        var results = new List<string>();

        while (commands.Count > 0)
        {
            results.Add(commands.Dequeue().Execute());
        }

        return results;
    }
}
