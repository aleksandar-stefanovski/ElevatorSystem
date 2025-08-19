namespace ElevatorSystem.Domain.Requests;

public class ElevatorRequest
{
    public int PassengerCurrentFloor { get; }

    public int RequestedFloorToGo { get; }

    public ElevatorRequest(int passengerCurrentFloor, int requestedFloorToGo)
    {
        if (passengerCurrentFloor < 0 || passengerCurrentFloor > 10)
            throw new ArgumentOutOfRangeException(nameof(passengerCurrentFloor), "Pickup must be between 0 and 10.");

        if (requestedFloorToGo < 0 || requestedFloorToGo > 10)
            throw new ArgumentOutOfRangeException(nameof(requestedFloorToGo), "Destination must be between 0 and 10.");

        if (passengerCurrentFloor == requestedFloorToGo)
            throw new ArgumentException("Pickup and destination floors cannot be the same.");

        PassengerCurrentFloor = passengerCurrentFloor;
        RequestedFloorToGo = requestedFloorToGo;
    }
}
