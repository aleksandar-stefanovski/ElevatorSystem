using ElevatorSystem.Domain.Interfaces;
using ElevatorSystem.Domain.Models;

namespace ElevatorSystem.Repository;

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
