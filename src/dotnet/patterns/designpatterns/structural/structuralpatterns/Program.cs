/*
 * Project: StructuralPatterns
 * Description:
 * This sample demonstrates the seven structural patterns in a modern C# console app.
 * Each pattern lives in its own class file with a short description and usage frequency.
 */

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Structural Patterns Sample");
        Console.WriteLine("==========================");

        new AdapterPattern().Run();
        new BridgePattern().Run();
        new CompositePattern().Run();
        new DecoratorPattern().Run();
        new FacadePattern().Run();
        new FlyweightPattern().Run();
        new ProxyPattern().Run();
    }
}
