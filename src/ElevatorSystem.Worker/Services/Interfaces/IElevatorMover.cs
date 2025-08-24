using ElevatorSystem.Worker.Domain.Models;

namespace ElevatorSystem.Worker.Services.Interfaces;

public interface IElevatorMover
{
    Task RunAsync(Elevator elevator, CancellationToken ct);
}