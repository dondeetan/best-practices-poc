/*
 * Factory Method
 * Description: Lets a base class define an operation while derived classes choose
 * which concrete product participates in that operation.
 * Usage frequency: Common in frameworks, exporters, parsers, serializers, and hosted
 * application extension points.
 */

internal sealed class FactoryMethodPattern
{
    public string Description => "Let a document exporter choose the formatter it needs.";

    public string UsageFrequency => "Common";

    public void Run()
    {
        ConsoleSection.Print("Factory Method");

        DocumentExporter exporter = new MarkdownExporter();
        Console.WriteLine(exporter.Export("Sprint summary"));
    }
}

internal abstract class DocumentExporter
{
    public string Export(string content)
    {
        var formatter = CreateFormatter();
        return formatter.Format(content);
    }

    protected abstract IContentFormatter CreateFormatter();
}

internal interface IContentFormatter
{
    string Format(string content);
}

internal sealed class MarkdownExporter : DocumentExporter
{
    protected override IContentFormatter CreateFormatter() => new MarkdownFormatter();
}

internal sealed class MarkdownFormatter : IContentFormatter
{
    public string Format(string content) => $"# {content}{Environment.NewLine}- Generated with MarkdownFormatter";
}
