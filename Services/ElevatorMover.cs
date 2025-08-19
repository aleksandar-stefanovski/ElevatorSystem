using ElevatorSystem.Configuration;
using ElevatorSystem.Domain.Enums;
using ElevatorSystem.Domain.Models;
using ElevatorSystem.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ElevatorSystem.Services;

public sealed class ElevatorMover : IElevatorMover
{
    private readonly ElevatorConfiguration _options;

    public ElevatorMover(IOptions<ElevatorConfiguration> options)
    {
        _options = options.Value;
    }

    public async Task RunAsync(Elevator elevator, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (IsAtQueuedStop(elevator))
            {
                await Task.Delay(TimeSpan.FromSeconds(elevator.DwellSeconds), ct);
                elevator.RemoveStopAtCurrent();

                continue;
            }

            if (elevator.CurrentDirection == Direction.Idle && elevator.HasStops)
            {
                bool hasUp = elevator.GetUpStops().Any(f => f > elevator.CurrentFloor);
                bool hasDown = elevator.GetDownStops().Any(f => f < elevator.CurrentFloor);

                if (hasUp)
                    elevator.CurrentDirection = Direction.Up;
                else if (hasDown)
                    elevator.CurrentDirection = Direction.Down;

                await Task.Delay(50, ct);
                continue;
            }

            if (elevator.CurrentDirection is Direction.Up or Direction.Down)
            {
                elevator.CurrentFloor += elevator.CurrentDirection == Direction.Up ? 1 : -1;
                elevator.CurrentFloor = Math.Clamp(elevator.CurrentFloor, _options.MinFloor, _options.MaxFloor);

                await Task.Delay(TimeSpan.FromSeconds(elevator.MoveSeconds), ct);
                continue;
            }

            await Task.Delay(200, ct);
        }
    }

    private static bool IsAtQueuedStop(Elevator elevator) =>
        elevator.GetUpStops().Contains(elevator.CurrentFloor) || elevator.GetDownStops().Contains(elevator.CurrentFloor);
}
