using ElevatorSystem.Worker.Configuration;
using ElevatorSystem.Worker.Domain.Requests;
using ElevatorSystem.Worker.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace ElevatorSystem.Worker.Services;

/// <summary>
/// Generates a continuous stream of random elevator requests.
/// Ensures pickup and destination floors differ. The delay between requests is based on RequestPeriodSeconds.
/// </summary>
public sealed class RequestProducer : IRequestProducer
{
    private readonly ElevatorConfiguration _options;
    private readonly Random _random;

    public RequestProducer(IOptions<ElevatorConfiguration> options)
    {
        _options = options.Value;
        _random = new Random(_options.RandomSeed); // for demo purposes
    }

    public async IAsyncEnumerable<ElevatorRequest> StreamAsync([EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            int passengerCurrentFloor = _random.Next(_options.MinFloor, _options.MaxFloor + 1);
            int requestedFloorToGo;

            // Retry until we pick a different destination.
            do
            {
                requestedFloorToGo = _random.Next(_options.MinFloor, _options.MaxFloor + 1);
            }
            while (requestedFloorToGo == passengerCurrentFloor);

            yield return new ElevatorRequest(passengerCurrentFloor, requestedFloorToGo);

            await Task.Delay(TimeSpan.FromMilliseconds(_options.RequestPeriodSeconds), ct);
        }
    }
}
