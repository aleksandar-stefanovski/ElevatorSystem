using ElevatorSystem.Worker.Domain.Enums;
using ElevatorSystem.Worker.Domain.Interfaces;

namespace ElevatorSystem.Worker.Presentation;

/// <summary>
/// Simple console renderer that periodically prints the state of all elevators.
/// For clarity it lists: current floor, direction, and queued up/down stops.
/// </summary>
public class ElevatorDisplay : IElevatorDisplay
{
    private readonly IElevatorRepository _elevatorRepository;

    public ElevatorDisplay(IElevatorRepository repo)
    {
        _elevatorRepository = repo;
    }

    public async Task DisplayAsync(CancellationToken token)
    {
        var start = DateTimeOffset.UtcNow;

        while (!token.IsCancellationRequested)
        {
            Console.Clear();
            var t = (int)(DateTimeOffset.UtcNow - start).TotalSeconds;
            Console.WriteLine($"Time={t:000}s | Elevator status (floors 0..9)\n");

            foreach (var elevator in _elevatorRepository.GetElevators().OrderBy(x => x.Id))
            {
                var elevatorCurrentDirection = elevator.CurrentDirection switch
                {
                    Direction.Up => "Up",
                    Direction.Down => "Down",
                    _ => "Idle"
                };

                var up = elevator.GetUpStops().ToArray();
                var down = elevator.GetDownStops().ToArray();

                Console.WriteLine(
                    $"Elevator{elevator.Id}: floor={elevator.CurrentFloor,2} direction={elevatorCurrentDirection,-4} " +
                    $"up=[{string.Join(",", up)}] down=[{string.Join(",", down)}]");
            }

            Console.WriteLine("\n(Ctrl+C to exit)");

            await Task.Delay(1000, token);
        }
    }
}
