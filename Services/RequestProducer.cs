using ElevatorSystem.Configuration;
using ElevatorSystem.Domain.Requests;
using ElevatorSystem.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace ElevatorSystem.Services;

public sealed class RequestProducer : IRequestProducer
{
    private readonly ElevatorConfiguration _options;
    private readonly Random _random;

    public RequestProducer(IOptions<ElevatorConfiguration> options)
    {
        _options = options.Value;
        _random = new Random(_options.RandomSeed);
    }

    public async IAsyncEnumerable<ElevatorRequest> StreamAsync([EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            int passengerCurrentFloor = _random.Next(_options.MinFloor, _options.MaxFloor + 1);
            int requestedFloorToGo;

            do
            {
                requestedFloorToGo = _random.Next(_options.MinFloor, _options.MaxFloor + 1);
            }
            while (requestedFloorToGo == passengerCurrentFloor);

            yield return new ElevatorRequest(passengerCurrentFloor, requestedFloorToGo);

            await Task.Delay(TimeSpan.FromSeconds(_options.RequestPeriodSeconds), ct);
        }
    }
}
