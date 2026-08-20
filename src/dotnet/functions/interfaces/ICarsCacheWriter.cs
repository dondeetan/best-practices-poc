using Functions.Entities;

namespace Functions.Interfaces;

// Dependency Inversion Principle (SOLID): the timer function depends on a cache writer abstraction, not Redis directly.
public interface ICarsCacheWriter
{
    Task WriteAsync(IReadOnlyCollection<Car> cars, CancellationToken cancellationToken);
}
