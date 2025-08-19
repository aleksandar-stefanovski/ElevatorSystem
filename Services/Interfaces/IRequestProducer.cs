using ElevatorSystem.Domain.Requests;

namespace ElevatorSystem.Services.Interfaces;

public interface IRequestProducer
{
    IAsyncEnumerable<ElevatorRequest> StreamAsync(CancellationToken ct);
}
