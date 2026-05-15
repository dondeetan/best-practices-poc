/*
 * Project: BehavioralPatterns
 * Description:
 * This sample demonstrates the eleven classic behavioral patterns in a modern .NET console app.
 * Each pattern lives in its own class file with a short description and usage frequency.
 */

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Behavioral Patterns Sample");
        Console.WriteLine("==========================");

        new ChainOfResponsibilityPattern().Run();
        new CommandPattern().Run();
        new InterpreterPattern().Run();
        new IteratorPattern().Run();
        new MediatorPattern().Run();
        new MementoPattern().Run();
        new ObserverPattern().Run();
        new StatePattern().Run();
        new StrategyPattern().Run();
        new TemplateMethodPattern().Run();
        new VisitorPattern().Run();
    }
}
