/*
 * Project: CreationalPatterns
 * Description:
 * This sample demonstrates the five classic creational patterns in a modern .NET console app.
 * Each pattern lives in its own class file with a short description and usage frequency.
 */

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Creational Patterns Sample");
        Console.WriteLine("==========================");

        new AbstractFactoryPattern().Run();
        new BuilderPattern().Run();
        new FactoryMethodPattern().Run();
        new PrototypePattern().Run();
        new SingletonPattern().Run();
    }
}
