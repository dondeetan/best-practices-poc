/*
 * Chain of Responsibility
 * Description: Passes a request through handlers until one handles it or the chain completes.
 * Usage frequency: Common in middleware, validation pipelines, authorization checks,
 * approval workflows, and request processing.
 */

internal sealed class ChainOfResponsibilityPattern
{
    public string Description => "Approve a release candidate through a sequence of checks.";

    public string UsageFrequency => "Common";

    public void Run()
    {
        ConsoleSection.Print("Chain of Responsibility");

        var pipeline = new TestsPassedCheck();
        pipeline
            .SetNext(new SecurityReviewCheck())
            .SetNext(new ProductApprovalCheck());

        var candidate = new ReleaseCandidate(TestsPassed: true, SecurityReviewed: true, ProductApproved: true);
        Console.WriteLine(pipeline.Handle(candidate));
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
