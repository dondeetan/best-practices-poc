/*
 * Facade
 * Description: Offers one simplified entry point over a more complex subsystem.
 * Usage frequency: Very common for service gateways, application services, orchestration
 * layers, SDK clients, and high-level workflow APIs.
 */

internal sealed class FacadePattern
{
    public string Description => "Coordinate build, test, and deployment through one release facade.";

    public string UsageFrequency => "Very common";

    public void Run()
    {
        ConsoleSection.Print("Facade");

        var facade = new ReleaseFacade(new BuildService(), new TestService(), new DeploymentService());
        Console.WriteLine(facade.Release("EmployeeApi"));
    }
}

internal sealed class BuildService
{
    public string Run(string projectName) => $"Build passed for {projectName}.";
}

internal sealed class TestService
{
    public string Run(string projectName) => $"Tests passed for {projectName}.";
}

internal sealed class DeploymentService
{
    public string Run(string projectName) => $"{projectName} deployed to staging.";
}

internal sealed class ReleaseFacade(BuildService buildService, TestService testService, DeploymentService deploymentService)
{
    public string Release(string projectName)
    {
        return string.Join(
            " ",
            buildService.Run(projectName),
            testService.Run(projectName),
            deploymentService.Run(projectName));
    }
}
