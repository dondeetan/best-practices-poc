using Functions.Entities;

namespace Functions.Interfaces;

// Adapter pattern: hides the remote Cars API protocol behind an application-specific client contract.
public interface ICarsApiClient
{
    Task<IReadOnlyCollection<Car>> GetCarsAsync(CancellationToken cancellationToken);
}
