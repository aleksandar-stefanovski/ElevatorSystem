using ElevatorSystem.Worker.Configuration;
using ElevatorSystem.Worker.Domain.Enums;
using ElevatorSystem.Worker.Domain.Models;
using ElevatorSystem.Worker.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ElevatorSystem.Worker.Services;

/// <summary>
/// Long‑running task per elevator that simulates physical movement & stop servicing.
/// Responsibilities:
///  - Detect arrival at a queued stop -> dwell, remove stop, recompute direction.
///  - When idle but stops exist -> choose a direction based on remaining queues.
///  - Advance one floor per MoveSeconds according to CurrentDirection.
/// The mover intentionally does not decide which stops to add; that is handled elsewhere.
/// </summary>
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
            // 1. Arrived at a scheduled stop? Simulate dwell (door open) then remove it.
            if (IsAtQueuedStop(elevator))
            {
                await Task.Delay(TimeSpan.FromSeconds(elevator.DwellSeconds), ct);
                elevator.RemoveStopAtCurrent();

                continue;
            }

            // 2. Idle but have queued work -> pick a direction based on available stops.
            if (elevator.CurrentDirection == Direction.Idle && elevator.HasStops)
            {
                bool hasUp = elevator.GetUpStops().Any(f => f > elevator.CurrentFloor);
                bool hasDown = elevator.GetDownStops().Any(f => f < elevator.CurrentFloor);

                if (hasUp)
                    elevator.CurrentDirection = Direction.Up;
                else if (hasDown)
                    elevator.CurrentDirection = Direction.Down;

                // Small pause to avoid hot loop & give chance for cancellation/testing determinism.
                await Task.Delay(_options.IdleCheckDelayMilliseconds, ct);
                continue;
            }

            // 3. Moving: wait MoveSeconds then adjust floor one step in chosen direction.
            if (elevator.CurrentDirection is Direction.Up or Direction.Down)
            {
                await Task.Delay(TimeSpan.FromSeconds(elevator.MoveSeconds), ct);

                elevator.CurrentFloor += elevator.CurrentDirection == Direction.Up ? 1 : -1;
                elevator.CurrentFloor = Math.Clamp(elevator.CurrentFloor, _options.MinFloor, _options.MaxFloor);

                continue;
            }

            await Task.Delay(_options.IdleSleepDelayMilliseconds, ct);
        }
    }

    private static bool IsAtQueuedStop(Elevator elevator) =>
        elevator.GetUpStops().Contains(elevator.CurrentFloor) || elevator.GetDownStops().Contains(elevator.CurrentFloor);
}
