using ElevatorSystem.Worker.Domain.Requests;

namespace ElevatorSystem.Worker.Services.Interfaces;

public interface IRequestProducer
{
    IAsyncEnumerable<ElevatorRequest> StreamAsync(CancellationToken ct);
}
