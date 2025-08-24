using ElevatorSystem.Worker.Domain.Models;

namespace ElevatorSystem.Worker.Domain.Interfaces;

/// <summary>
/// Simple read-only access abstraction for the current set of elevators.
/// </summary>
public interface IElevatorRepository
{
    IReadOnlyList<Elevator> GetElevators();
}
