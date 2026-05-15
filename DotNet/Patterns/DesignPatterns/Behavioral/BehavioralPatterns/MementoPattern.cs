/*
 * Memento
 * Description: Captures and restores object state without exposing internal details.
 * Usage frequency: Occasional; useful for undo, drafts, checkpoints, simulations,
 * editors, and rollback-friendly workflows.
 */

internal sealed class MementoPattern
{
    public string Description => "Snapshot and restore a deployment plan editor.";

    public string UsageFrequency => "Occasional";

    public void Run()
    {
        ConsoleSection.Print("Memento");

        var editor = new DeploymentPlanEditor();
        editor.Update("Initial rollout plan");
        var snapshot = editor.CreateSnapshot();
        editor.Update("Revised rollout plan with feature flag fallback");
        editor.Restore(snapshot);

        Console.WriteLine(editor.CurrentPlan);
    }
}

internal sealed class DeploymentPlanEditor
{
    public string CurrentPlan { get; private set; } = "No plan yet";

    public void Update(string plan) => CurrentPlan = plan;

    public DeploymentPlanSnapshot CreateSnapshot() => new(CurrentPlan);

    public void Restore(DeploymentPlanSnapshot snapshot) => CurrentPlan = snapshot.Plan;
}

internal sealed record DeploymentPlanSnapshot(string Plan);
