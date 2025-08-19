using ElevatorSystem.Domain.Models;

namespace ElevatorSystem.Services.Interfaces;

public interface IElevatorMover
{
    Task RunAsync(Elevator elevator, CancellationToken ct);
}