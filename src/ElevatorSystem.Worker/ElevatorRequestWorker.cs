using ElevatorSystem.Worker.Domain.Interfaces;
using ElevatorSystem.Worker.Presentation;
using ElevatorSystem.Worker.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ElevatorSystem.Worker;

/// <summary>
/// Background host service that wires together:
///  - Per-elevator mover simulation tasks
///  - Console display refresher
///  - Random request stream consumption & dispatch
/// It simply starts the tasks and then awaits their completion (which is never under normal run).
/// </summary>
public sealed class ElevatorRequestWorker : BackgroundService
{
    private readonly IElevatorRepository _elevatorRepository;
    private readonly IElevatorDisplay _display;
    private readonly IElevatorService _elevatorService;
    private readonly IRequestProducer _producer;
    private readonly IElevatorMover _mover;

    public ElevatorRequestWorker(
        IElevatorRepository elevatorRepository,
        IElevatorDisplay display,
        IElevatorService elevatorService,
        IRequestProducer producer,
        IElevatorMover mover)
    {
        _elevatorRepository = elevatorRepository;
        _display = display;
        _elevatorService = elevatorService;
        _producer = producer;
        _mover = mover;
    }

    protected override Task ExecuteAsync(CancellationToken token)
    {
        // Start a mover simulation per elevator.
        var tasks = _elevatorRepository
            .GetElevators()
            .Select(elevator => _mover.RunAsync(elevator, token))
            .ToList();

        // Add display & request consumer tasks.
        tasks.Add(_display.DisplayAsync(token));
        tasks.Add(ConsumeRequestsAsync(token));

        return Task.WhenAll(tasks);
    }

    private async Task ConsumeRequestsAsync(CancellationToken ct)
    {
        await foreach (var req in _producer.StreamAsync(ct))
        {
            _elevatorService.AssignElevator(req); // fire-and-forget assignment
        }
    }
}
