using ElevatorSystem.Domain.Interfaces;
using ElevatorSystem.Presentation;
using ElevatorSystem.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ElevatorSystem;

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
        var tasks = _elevatorRepository
            .GetElevators()
            .Select(elevator => _mover.RunAsync(elevator, token))
            .ToList();

        tasks.Add(_display.DisplayAsync(token));
        tasks.Add(ConsumeRequestsAsync(token));

        return Task.WhenAll(tasks);
    }

    private async Task ConsumeRequestsAsync(CancellationToken ct)
    {
        await foreach (var req in _producer.StreamAsync(ct))
        {
            _elevatorService.AssignElevator(req);
        }
    }
}
