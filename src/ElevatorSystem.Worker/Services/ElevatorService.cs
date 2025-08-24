using ElevatorSystem.Worker.Domain.Interfaces;
using ElevatorSystem.Worker.Domain.Models;
using ElevatorSystem.Worker.Domain.Requests;
using ElevatorSystem.Worker.Services.Interfaces;

namespace ElevatorSystem.Worker.Services;

/// <summary>
/// Service used by the worker to process an incoming elevator request.
/// </summary>
public class ElevatorService : IElevatorService
{
    private readonly IElevatorRepository _elevatorRepository;

    public ElevatorService(IElevatorRepository elevatorRepository)
    {
        _elevatorRepository = elevatorRepository ?? throw new ArgumentNullException(nameof(elevatorRepository));
    }

    /// <summary>
    /// Assign the most suitable elevator and enqueue pickup + destination.
    /// Throws if pickup == destination
    /// </summary>
    public Elevator AssignElevator(ElevatorRequest request)
    {
        var passengerCurrentFloor = request.PassengerCurrentFloor;
        var requestedFloorToGo = request.RequestedFloorToGo;

        if (passengerCurrentFloor == requestedFloorToGo)
            throw new ArgumentException("Pickup and destination cannot be the same.");

        var bestResult = ElevatorProcessor.SelectBest(_elevatorRepository.GetElevators(), passengerCurrentFloor);

        ElevatorProcessor.Enqueue(bestResult, passengerCurrentFloor, requestedFloorToGo);

        return bestResult;
    }
}
