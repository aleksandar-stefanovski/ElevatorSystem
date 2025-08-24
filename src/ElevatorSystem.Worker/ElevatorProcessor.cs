using ElevatorSystem.Worker.Domain.Enums;
using ElevatorSystem.Worker.Domain.Models;

namespace ElevatorSystem.Worker;

/// <summary>
/// Stateless helper with the main decision logic:
///  - choose which elevator should take a new pickup
///  - estimate how long until an elevator can reach a passenger
///  - enqueue pickup + destination stops and set initial direction when idle
/// </summary>
public class ElevatorProcessor
{
    public static Elevator SelectBest(IEnumerable<Elevator> elevators, int passengerCurrentFloor)
    {
        // 1. Lowest estimated pickup time
        // 2. If same, physically closer right now
        // 3. If same, fewer total queued stops
        // 4. If same, lower id for deterministic outcome
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
        // If already at the passenger floor we only pay dwell (door open) time.
        if (passengerCurrentFloor == elevator.CurrentFloor)
            return elevator.DwellSeconds;

        // Delegate to direction specific estimator.
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
        // Add pickup then destination. Elevator.AddStop handles sorting + no duplicates.
        elevator.AddStop(passengerCurrentFloor);
        elevator.AddStop(requestedFloorToGo);

        // If the car was idle decide what direction to start moving.
        if (elevator.CurrentDirection == Direction.Idle)
        {
            if (passengerCurrentFloor > elevator.CurrentFloor)
                elevator.CurrentDirection = Direction.Up;

            else if (passengerCurrentFloor < elevator.CurrentFloor)
                elevator.CurrentDirection = Direction.Down;

            else
                // Passenger is already here: direction is based on where they want to go next.
                elevator.CurrentDirection = requestedFloorToGo > passengerCurrentFloor ? Direction.Up : Direction.Down;
        }
    }

    // Straight line distance * move time + one dwell at pickup.
    private static int EstimateIdle(Elevator elevator, int passengerCurrentFloor)
    {
        int moveSeconds = elevator.MoveSeconds;
        int dwellSeconds = elevator.DwellSeconds;
        return Math.Abs(passengerCurrentFloor - elevator.CurrentFloor) * moveSeconds + dwellSeconds;
    }

    //  A) Passenger is ahead (same or higher floor) => travel upward directly.
    //     Add dwell for any queued up stops we must service on the way, plus pickup dwell.
    //  B) Passenger is behind (lower floor) => finish current upward plan first (go to furthest queued up stop),
    //     dwell at each remaining up stop, then travel back down to the passenger and dwell once at pickup.
    private static int EstimateUp(Elevator elevator, int passengerCurrentFloor)
    {
        int moveSeconds = elevator.MoveSeconds;
        int dwellSeconds = elevator.DwellSeconds;
        int elevatorCurrentFloor = elevator.CurrentFloor;

        var up = elevator.GetUpStops();
        int maxUp = up.Count > 0 ? up[^1] : elevatorCurrentFloor; // highest stop we still intend to reach

        // Passenger is ahead or level with current upward travel.
        if (passengerCurrentFloor >= elevatorCurrentFloor)
        {
            int travel = (passengerCurrentFloor - elevatorCurrentFloor) * moveSeconds;
            // Stops strictly between current and passenger floor we must open at before pickup.
            int dwellOnWay = up.Count(s => s > elevatorCurrentFloor && s < passengerCurrentFloor);
            return travel + dwellOnWay * dwellSeconds + dwellSeconds; // include pickup dwell at the end
        }

        // Passenger is behind -> complete upward route then reverse.
        int travelUp = (maxUp - elevatorCurrentFloor) * moveSeconds;          // finish going up
        int dwellUp = up.Count(s => s > elevatorCurrentFloor) * dwellSeconds;  // dwell for each remaining up stop
        int travelDown = (maxUp - passengerCurrentFloor) * moveSeconds;        // come back down to passenger
        return travelUp + dwellUp + travelDown + dwellSeconds;                 // final dwell at pickup
    }

    //  A) Passenger is ahead (same or lower floor) => direct down path + dwell at intermediate down stops.
    //  B) Passenger is above => finish all planned down stops, then travel up to passenger.
    private static int EstimateDown(Elevator elevator, int passengerCurrentFloor)
    {
        int moveSeconds = elevator.MoveSeconds;
        int dwellSeconds = elevator.DwellSeconds;
        int elevatorCurrentFloor = elevator.CurrentFloor;

        var down = elevator.GetDownStops();
        int minDown = down.Count > 0 ? down[^1] : elevatorCurrentFloor; // lowest stop in plan (list stored descending)

        // Passenger lies ahead on the current downward path.
        if (passengerCurrentFloor <= elevatorCurrentFloor)
        {
            int travel = (elevatorCurrentFloor - passengerCurrentFloor) * moveSeconds;
            // Stops between current and passenger floor we must service first.
            int dwellOnWay = down.Count(s => s < elevatorCurrentFloor && s > passengerCurrentFloor);
            return travel + dwellOnWay * dwellSeconds + dwellSeconds; // include pickup dwell
        }

        // Passenger above -> finish down trip then reverse upward.
        int travelDown = (elevatorCurrentFloor - minDown) * moveSeconds;
        int dwellDown = down.Count(s => s < elevatorCurrentFloor) * dwellSeconds;
        int travelUp = (passengerCurrentFloor - minDown) * moveSeconds;

        return travelDown + dwellDown + travelUp + dwellSeconds;
    }
}
