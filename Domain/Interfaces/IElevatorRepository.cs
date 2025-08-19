using ElevatorSystem.Domain.Models;

namespace ElevatorSystem.Domain.Interfaces;

public interface IElevatorRepository
{
    IReadOnlyList<Elevator> GetElevators();
}
