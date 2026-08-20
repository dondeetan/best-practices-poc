/*
 * Open/Closed Principle
 * Description: Software entities should be open for extension but closed for modification.
 * This sample adds compensation rules without changing the calculator.
 * Usage frequency: Common, especially in business rules, pricing, validation,
 * workflow steps, and plugin-style application features.
 */

internal sealed class OpenClosedPrinciple
{
    public string Description => "Add new compensation rules without changing the calculator.";

    public string UsageFrequency => "Common";

    public void Run()
    {
        ConsoleSection.Print("Open/Closed Principle");

        var teammate = new TeamMember("Avery", "Engineering Manager", YearsAtCompany: 4, BaseSalary: 125_000m);
        var rules = new ICompensationRule[]
        {
            new BaseSalaryRule(),
            new TenureBonusRule(),
            new LeadershipBonusRule()
        };

        var calculator = new CompensationCalculator(rules);
        Console.WriteLine($"Total compensation for {teammate.Name}: {calculator.Calculate(teammate):C0}");
    }
}

internal sealed record TeamMember(string Name, string Role, int YearsAtCompany, decimal BaseSalary);

internal interface ICompensationRule
{
    decimal Apply(decimal runningTotal, TeamMember member);
}

internal sealed class CompensationCalculator(IEnumerable<ICompensationRule> rules)
{
    private readonly IReadOnlyCollection<ICompensationRule> rules = rules.ToArray();

    public decimal Calculate(TeamMember member)
    {
        var total = 0m;

        foreach (var rule in rules)
        {
            total = rule.Apply(total, member);
        }

        return total;
    }
}

internal sealed class BaseSalaryRule : ICompensationRule
{
    public decimal Apply(decimal runningTotal, TeamMember member) => runningTotal + member.BaseSalary;
}

internal sealed class TenureBonusRule : ICompensationRule
{
    public decimal Apply(decimal runningTotal, TeamMember member) =>
        runningTotal + (member.YearsAtCompany * 1_500m);
}

internal sealed class LeadershipBonusRule : ICompensationRule
{
    public decimal Apply(decimal runningTotal, TeamMember member) =>
        member.Role.Contains("Manager", StringComparison.OrdinalIgnoreCase)
            ? runningTotal + 10_000m
            : runningTotal;
}
