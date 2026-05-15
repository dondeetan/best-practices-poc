/*
 * Interpreter
 * Description: Evaluates a small grammar by composing expression objects.
 * Usage frequency: Rare in everyday application code, but useful for rule engines,
 * filters, query languages, and policy expressions.
 */

internal sealed class InterpreterPattern
{
    public string Description => "Evaluate a release rule made from boolean expressions.";

    public string UsageFrequency => "Rare";

    public void Run()
    {
        ConsoleSection.Print("Interpreter");

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
