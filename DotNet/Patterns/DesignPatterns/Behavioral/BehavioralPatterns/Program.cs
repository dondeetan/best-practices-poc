/*
 * Project: BehavioralPatterns
 * Description:
 * This sample demonstrates the eleven classic behavioral patterns in a modern .NET console app:
 * Chain of Responsibility, Command, Interpreter, Iterator, Mediator, Memento, Observer,
 * State, Strategy, Template Method, and Visitor.
 * The examples show how behavior can be composed, extended, and coordinated cleanly.
 */

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Behavioral Patterns Sample");
        Console.WriteLine("==========================");

        RunChainOfResponsibility();
        RunCommand();
        RunInterpreter();
        RunIterator();
        RunMediator();
        RunMemento();
        RunObserver();
        RunState();
        RunStrategy();
        RunTemplateMethod();
        RunVisitor();
    }

    // Chain of Responsibility passes a request through handlers until one fails or the chain completes.
    private static void RunChainOfResponsibility()
    {
        PrintSection("Chain of Responsibility");

        var pipeline = new TestsPassedCheck();
        pipeline
            .SetNext(new SecurityReviewCheck())
            .SetNext(new ProductApprovalCheck());

        var candidate = new ReleaseCandidate(TestsPassed: true, SecurityReviewed: true, ProductApproved: true);
        Console.WriteLine(pipeline.Handle(candidate));
    }

    // Command encapsulates a request so it can be queued, logged, or retried.
    private static void RunCommand()
    {
        PrintSection("Command");

        var invoker = new DeploymentQueue();
        invoker.Enqueue(new DeployEnvironmentCommand("staging"));
        invoker.Enqueue(new DeployEnvironmentCommand("production"));

        foreach (var result in invoker.RunAll())
        {
            Console.WriteLine(result);
        }
    }

    // Interpreter evaluates a small grammar by composing expression objects.
    private static void RunInterpreter()
    {
        PrintSection("Interpreter");

        IBooleanExpression expression =
            new OrExpression(
                new AndExpression(
                    new VariableExpression("approved"),
                    new VariableExpression("testsPassed")),
                new VariableExpression("hotfix"));

        var context = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["approved"] = true,
            ["testsPassed"] = true,
            ["hotfix"] = false
        };

        Console.WriteLine($"Release rule evaluated to: {expression.Evaluate(context)}");
    }

    // Iterator exposes traversal logic without leaking the collection's internal representation.
    private static void RunIterator()
    {
        PrintSection("Iterator");

        var backlog = new SprintBacklog();
        backlog.Add(new BacklogCard("Protect admin endpoint", 1));
        backlog.Add(new BacklogCard("Improve cache invalidation", 2));
        backlog.Add(new BacklogCard("Refresh README diagrams", 3));

        foreach (var card in backlog.GetByPriority(maxPriority: 2))
        {
            Console.WriteLine($"{card.Title} (priority {card.Priority})");
        }
    }

    // Mediator centralizes collaboration so peers stay loosely coupled.
    private static void RunMediator()
    {
        PrintSection("Mediator");

        var mediator = new StandupMediator();
        var developer = new StandupParticipant("Developer", mediator);
        var qa = new StandupParticipant("QA", mediator);

        mediator.Register(developer);
        mediator.Register(qa);

        developer.Send("API changes are ready for regression testing.");
    }

    // Memento captures and restores object state without exposing internal details.
    private static void RunMemento()
    {
        PrintSection("Memento");

        var editor = new DeploymentPlanEditor();
        editor.Update("Initial rollout plan");
        var snapshot = editor.CreateSnapshot();
        editor.Update("Revised rollout plan with feature flag fallback");
        editor.Restore(snapshot);

        Console.WriteLine(editor.CurrentPlan);
    }

    // Observer pushes updates to subscribers when the subject changes.
    private static void RunObserver()
    {
        PrintSection("Observer");

        var pipeline = new BuildPipeline();
        pipeline.Subscribe(new ConsoleBuildSubscriber("Ops"));
        pipeline.Subscribe(new ConsoleBuildSubscriber("QA"));
        pipeline.Complete("Employee API");
    }

    // State changes behavior by swapping the active state object.
    private static void RunState()
    {
        PrintSection("State");

        var item = new WorkItemContext();
        Console.WriteLine(item.Status);
        item.Advance();
        Console.WriteLine(item.Status);
        item.Advance();
        Console.WriteLine(item.Status);
    }

    // Strategy selects an algorithm at runtime behind one shared contract.
    private static void RunStrategy()
    {
        PrintSection("Strategy");

        var planner = new IncidentResponsePlanner(new PagingResponseStrategy());
        Console.WriteLine(planner.Execute("Critical outage"));
    }

    // Template Method fixes the workflow skeleton but lets subclasses customize steps.
    private static void RunTemplateMethod()
    {
        PrintSection("Template Method");

        var pipeline = new PullRequestQualityPipeline();
        Console.WriteLine(pipeline.Run());
    }

    // Visitor applies new operations across an object structure without changing the element classes.
    private static void RunVisitor()
    {
        PrintSection("Visitor");

        var components = new IArchitectureComponent[]
        {
            new ApiComponent("Employee API"),
            new FunctionComponent("Cars Sync Function")
        };

        var visitor = new ArchitectureSummaryVisitor();
        foreach (var component in components)
        {
            component.Accept(visitor);
        }

        Console.WriteLine(visitor.GetSummary());
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));
    }
}

internal sealed record ReleaseCandidate(bool TestsPassed, bool SecurityReviewed, bool ProductApproved);

internal abstract class ApprovalHandler
{
    private ApprovalHandler? next;

    public ApprovalHandler SetNext(ApprovalHandler handler)
    {
        next = handler;
        return handler;
    }

    public string Handle(ReleaseCandidate candidate)
    {
        var decision = Evaluate(candidate);
        return decision ?? next?.Handle(candidate) ?? "Release candidate approved.";
    }

    protected abstract string? Evaluate(ReleaseCandidate candidate);
}

internal sealed class TestsPassedCheck : ApprovalHandler
{
    protected override string? Evaluate(ReleaseCandidate candidate) =>
        candidate.TestsPassed ? null : "Rejected: automated tests must pass first.";
}

internal sealed class SecurityReviewCheck : ApprovalHandler
{
    protected override string? Evaluate(ReleaseCandidate candidate) =>
        candidate.SecurityReviewed ? null : "Rejected: security review is still pending.";
}

internal sealed class ProductApprovalCheck : ApprovalHandler
{
    protected override string? Evaluate(ReleaseCandidate candidate) =>
        candidate.ProductApproved ? null : "Rejected: product owner approval is still pending.";
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

internal interface IBooleanExpression
{
    bool Evaluate(IReadOnlyDictionary<string, bool> context);
}

internal sealed class VariableExpression(string key) : IBooleanExpression
{
    public bool Evaluate(IReadOnlyDictionary<string, bool> context) => context.TryGetValue(key, out var value) && value;
}

internal sealed class AndExpression(IBooleanExpression left, IBooleanExpression right) : IBooleanExpression
{
    public bool Evaluate(IReadOnlyDictionary<string, bool> context) => left.Evaluate(context) && right.Evaluate(context);
}

internal sealed class OrExpression(IBooleanExpression left, IBooleanExpression right) : IBooleanExpression
{
    public bool Evaluate(IReadOnlyDictionary<string, bool> context) => left.Evaluate(context) || right.Evaluate(context);
}

internal sealed record BacklogCard(string Title, int Priority);

internal sealed class SprintBacklog
{
    private readonly List<BacklogCard> cards = new();

    public void Add(BacklogCard card) => cards.Add(card);

    public IEnumerable<BacklogCard> GetByPriority(int maxPriority)
    {
        foreach (var card in cards.Where(card => card.Priority <= maxPriority))
        {
            yield return card;
        }
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

internal sealed class DeploymentPlanEditor
{
    public string CurrentPlan { get; private set; } = "No plan yet";

    public void Update(string plan) => CurrentPlan = plan;

    public DeploymentPlanSnapshot CreateSnapshot() => new(CurrentPlan);

    public void Restore(DeploymentPlanSnapshot snapshot) => CurrentPlan = snapshot.Plan;
}

internal sealed record DeploymentPlanSnapshot(string Plan);

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

internal sealed class WorkItemContext
{
    private IWorkItemState state = new TodoState();

    public string Status => state.Name;

    public void Advance() => state = state.Advance();
}

internal interface IWorkItemState
{
    string Name { get; }
    IWorkItemState Advance();
}

internal sealed class TodoState : IWorkItemState
{
    public string Name => "To Do";

    public IWorkItemState Advance() => new InProgressState();
}

internal sealed class InProgressState : IWorkItemState
{
    public string Name => "In Progress";

    public IWorkItemState Advance() => new DoneState();
}

internal sealed class DoneState : IWorkItemState
{
    public string Name => "Done";

    public IWorkItemState Advance() => this;
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

internal abstract class QualityPipelineTemplate
{
    public string Run()
    {
        var artifacts = CollectArtifacts();
        var checks = ExecuteChecks();
        return PublishOutcome(artifacts, checks);
    }

    protected abstract string CollectArtifacts();
    protected abstract string ExecuteChecks();
    protected abstract string PublishOutcome(string artifacts, string checks);
}

internal sealed class PullRequestQualityPipeline : QualityPipelineTemplate
{
    protected override string CollectArtifacts() => "Artifacts collected from pull request.";

    protected override string ExecuteChecks() => "Lint, tests, and security scan passed.";

    protected override string PublishOutcome(string artifacts, string checks) => $"{artifacts} {checks}";
}

internal interface IArchitectureComponent
{
    void Accept(IArchitectureVisitor visitor);
}

internal sealed class ApiComponent(string name) : IArchitectureComponent
{
    public string Name { get; } = name;

    public void Accept(IArchitectureVisitor visitor) => visitor.Visit(this);
}

internal sealed class FunctionComponent(string name) : IArchitectureComponent
{
    public string Name { get; } = name;

    public void Accept(IArchitectureVisitor visitor) => visitor.Visit(this);
}

internal interface IArchitectureVisitor
{
    void Visit(ApiComponent component);
    void Visit(FunctionComponent component);
}

internal sealed class ArchitectureSummaryVisitor : IArchitectureVisitor
{
    private readonly List<string> lines = new();

    public void Visit(ApiComponent component) => lines.Add($"API component reviewed: {component.Name}");

    public void Visit(FunctionComponent component) => lines.Add($"Function component reviewed: {component.Name}");

    public string GetSummary() => string.Join(" | ", lines);
}
