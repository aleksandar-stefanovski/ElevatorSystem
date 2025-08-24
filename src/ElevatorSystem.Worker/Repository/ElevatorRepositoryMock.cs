using ElevatorSystem.Worker.Domain.Interfaces;
using ElevatorSystem.Worker.Domain.Models;

namespace ElevatorSystem.Worker.Repository;

/// <summary>
/// Very small in-memory repository used for simulation / demo purposes.
/// In a real system this would be replaced with a persistence-backed implementation.
/// </summary>
public class ElevatorRepositoryMock : IElevatorRepository
{
    private readonly List<Elevator> _elevators;

    public ElevatorRepositoryMock()
    {
        _elevators =
            [
                new Elevator { Id = 1 },
                new Elevator { Id = 2 },
                new Elevator { Id = 3 },
                new Elevator { Id = 4 }
            ];
    }

    public IReadOnlyList<Elevator> GetElevators() => _elevators;
}
