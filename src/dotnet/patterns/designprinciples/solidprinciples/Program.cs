/*
 * Project: SolidPrinciples
 * Description:
 * This console sample demonstrates the SOLID principles with small, modern C# examples.
 * Each principle lives in its own class file with a short description and usage frequency.
 */

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("SOLID Principles Sample");
        Console.WriteLine("=======================");

        var repository = new InMemoryWorkItemRepository();
        repository.Add(new WorkItem("Harden API authentication", 5, true));
        repository.Add(new WorkItem("Document retry strategy", 2, false));
        repository.Add(new WorkItem("Refactor cache adapter", 3, true));

        new SingleResponsibilityPrinciple().Run(repository);
        new OpenClosedPrinciple().Run();
        new LiskovSubstitutionPrinciple().Run();
        new InterfaceSegregationPrinciple().Run();
        new DependencyInversionPrinciple().Run(repository);
    }
}
