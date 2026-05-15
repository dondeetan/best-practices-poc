/*
 * Abstract Factory
 * Description: Creates families of related objects without exposing concrete types.
 * Usage frequency: Occasional; useful for cross-cloud providers, UI kits, persistence
 * providers, or test fixtures that must swap complete product families together.
 */

internal sealed class AbstractFactoryPattern
{
    public string Description => "Create related cloud clients through one provider factory.";

    public string UsageFrequency => "Occasional";

    public void Run()
    {
        ConsoleSection.Print("Abstract Factory");

        ICloudProvisioningFactory factory = new AzureProvisioningFactory();
        var queue = factory.CreateQueueClient();
        var storage = factory.CreateStorageClient();

        Console.WriteLine(queue.Describe());
        Console.WriteLine(storage.Describe());
    }
}

internal interface ICloudProvisioningFactory
{
    IQueueClient CreateQueueClient();
    IStorageClient CreateStorageClient();
}

internal interface IQueueClient
{
    string Describe();
}

internal interface IStorageClient
{
    string Describe();
}

internal sealed class AzureProvisioningFactory : ICloudProvisioningFactory
{
    public IQueueClient CreateQueueClient() => new AzureQueueClient();

    public IStorageClient CreateStorageClient() => new AzureBlobStorageClient();
}

internal sealed class AzureQueueClient : IQueueClient
{
    public string Describe() => "Azure Queue client provisioned for asynchronous work.";
}

internal sealed class AzureBlobStorageClient : IStorageClient
{
    public string Describe() => "Azure Blob Storage client provisioned for file retention.";
}
