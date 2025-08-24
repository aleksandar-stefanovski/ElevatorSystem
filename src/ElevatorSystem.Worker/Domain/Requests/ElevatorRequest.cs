namespace ElevatorSystem.Worker.Domain.Requests;

/// <summary>
/// Immutable value object representing a single passenger call: origin + destination.
/// Constructor validates that origin and destination are different.
/// </summary>
public class ElevatorRequest
{
    public int PassengerCurrentFloor { get; }

    public int RequestedFloorToGo { get; }

    public ElevatorRequest(int passengerCurrentFloor, int requestedFloorToGo)
    {
        if (passengerCurrentFloor == requestedFloorToGo)
            throw new ArgumentException("Pickup and destination floors cannot be the same.");

        PassengerCurrentFloor = passengerCurrentFloor;
        RequestedFloorToGo = requestedFloorToGo;
    }
}
