using ElevatorSystem.Domain.Enums;
using ElevatorSystem.Domain.Models;

namespace ElevatorSystem;

public class ElevatorProcessor
{
    public static Elevator SelectBest(IEnumerable<Elevator> elevators, int passengerCurrentFloor)
    {
        var result = elevators
            .OrderBy(elevator => EstimatePickupSeconds(elevator, passengerCurrentFloor))
            .ThenBy(elevator => Math.Abs(elevator.CurrentFloor - passengerCurrentFloor))
            .ThenBy(elevator => elevator.UpStopsCount() + elevator.DownStopsCount())
            .ThenBy(elevator => elevator.Id)
            .First();

        return result;
    }

    public static int EstimatePickupSeconds(Elevator elevator, int passengerCurrentFloor)
    {
        if (passengerCurrentFloor == elevator.CurrentFloor)
            return elevator.DwellSeconds;

        return elevator.CurrentDirection switch
        {
            Direction.Idle => EstimateIdle(elevator, passengerCurrentFloor),
            Direction.Up => EstimateUp(elevator, passengerCurrentFloor),
            Direction.Down => EstimateDown(elevator, passengerCurrentFloor),
            _ => throw new InvalidOperationException()
        };
    }

    public static void Enqueue(Elevator elevator, int passengerCurrentFloor, int requestedFloorToGo)
    {
        elevator.AddStop(passengerCurrentFloor);
        elevator.AddStop(requestedFloorToGo);

        if (elevator.CurrentDirection == Direction.Idle)
        {
            if (passengerCurrentFloor > elevator.CurrentFloor)
                elevator.CurrentDirection = Direction.Up;

            else if (passengerCurrentFloor < elevator.CurrentFloor)
                elevator.CurrentDirection = Direction.Down;

            else
                elevator.CurrentDirection = requestedFloorToGo > passengerCurrentFloor ? Direction.Up : Direction.Down;
        }
    }

    private static int EstimateIdle(Elevator elevator, int passengerCurrentFloor)
    {
        int moveSeconds = elevator.MoveSeconds;
        int dwellSeconds = elevator.DwellSeconds;
        return Math.Abs(passengerCurrentFloor - elevator.CurrentFloor) * moveSeconds + dwellSeconds;
    }

    private static int EstimateUp(Elevator elevator, int passengerCurrentFloor)
    {
        int moveSeconds = elevator.MoveSeconds;
        int dwellSeconds = elevator.DwellSeconds;

        int elevatorCurrentFloor = elevator.CurrentFloor;

        var up = elevator.GetUpStops();
        int maxUp = up.Count > 0 ? up[^1] : elevatorCurrentFloor;

        if (passengerCurrentFloor >= elevatorCurrentFloor)
        {
            int travel = (passengerCurrentFloor - elevatorCurrentFloor) * moveSeconds;
            int dwellOnWay = up.Count(s => s > elevatorCurrentFloor && s < passengerCurrentFloor);

            return travel + dwellOnWay * dwellSeconds + dwellSeconds;
        }

        int travelUp = (maxUp - elevatorCurrentFloor) * moveSeconds;
        int dwellUp = up.Count(s => s > elevatorCurrentFloor) * dwellSeconds;
        int travelDown = (maxUp - passengerCurrentFloor) * moveSeconds;

        return travelUp + dwellUp + travelDown + dwellSeconds;
    }

    private static int EstimateDown(Elevator elevator, int passengerCurrentFloor)
    {
        int moveSeconds = elevator.MoveSeconds;
        int dwellSeconds = elevator.DwellSeconds;

        int elevatorCurrentFloor = elevator.CurrentFloor;

        var down = elevator.GetDownStops();
        int minDown = down.Count > 0 ? down[^1] : elevatorCurrentFloor;

        if (passengerCurrentFloor <= elevatorCurrentFloor)
        {
            int travel = (elevatorCurrentFloor - passengerCurrentFloor) * moveSeconds;
            int dwellOnWay = down.Count(s => s < elevatorCurrentFloor && s > passengerCurrentFloor);

            return travel + dwellOnWay * dwellSeconds + dwellSeconds;
        }

        int travelDown = (elevatorCurrentFloor - minDown) * moveSeconds;
        int dwellDown = down.Count(s => s < elevatorCurrentFloor) * dwellSeconds;
        int travelUp = (passengerCurrentFloor - minDown) * moveSeconds;

        return travelDown + dwellDown + travelUp + dwellSeconds;
    }
}
