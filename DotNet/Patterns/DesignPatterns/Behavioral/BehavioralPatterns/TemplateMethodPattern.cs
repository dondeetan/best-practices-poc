/*
 * Template Method
 * Description: Fixes the workflow skeleton while allowing subclasses to customize steps.
 * Usage frequency: Occasional in modern application code; more common in frameworks,
 * test fixtures, import/export pipelines, and base classes with controlled extension points.
 */

internal sealed class TemplateMethodPattern
{
    public string Description => "Run a quality pipeline with overridable workflow steps.";

    public string UsageFrequency => "Occasional";

    public void Run()
    {
        ConsoleSection.Print("Template Method");

        var pipeline = new PullRequestQualityPipeline();
        Console.WriteLine(pipeline.Run());
    }
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
