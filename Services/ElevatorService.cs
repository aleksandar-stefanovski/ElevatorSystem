using ElevatorSystem.Domain.Interfaces;
using ElevatorSystem.Domain.Models;
using ElevatorSystem.Domain.Requests;
using ElevatorSystem.Services.Interfaces;

namespace ElevatorSystem.Services;

public class ElevatorService : IElevatorService
{
    private readonly IElevatorRepository _elevatorRepository;

    public ElevatorService(IElevatorRepository elevatorRepository)
    {
        _elevatorRepository = elevatorRepository ?? throw new ArgumentNullException(nameof(elevatorRepository));
    }

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
