using ElevatorSystem.Worker;
using ElevatorSystem.Worker.Domain.Enums;
using ElevatorSystem.Worker.Domain.Models;
using FluentAssertions;

namespace ElevatorSystem.Tests.Processor;

public class ElevatorProcessorTests
{
    [Fact]
    public void EstimatePickupSeconds_Idle_Is_Distance_Times_Move_Plus_Dwell()
    {
        var elevator = new Elevator { Id = 1, CurrentFloor = 0, MoveSeconds = 10, DwellSeconds = 10 };

        ElevatorProcessor.EstimatePickupSeconds(elevator, 3)
            .Should()
            .Be(3 * 10 + 10);
    }

    [Fact]
    public void EstimatePickupSeconds_Up_WithPassengerAhead_IncludesDwellAtEarlierStops()
    {
        var elevator = new Elevator { Id = 1, CurrentFloor = 1, CurrentDirection = Direction.Up, MoveSeconds = 10, DwellSeconds = 10 };
        elevator.AddStop(2);
        elevator.AddStop(5);

        // passenger at 4 -> travel 30s (floors 1->4) + dwell for stop at 2 (10) + dwell at pickup (10)
        ElevatorProcessor.EstimatePickupSeconds(elevator, 4)
            .Should()
            .Be(30 + 10 + 10);
    }

    [Fact]
    public void EstimatePickupSeconds_Up_When_Passenger_Behind_Requires_Up_Then_Down()
    {
        var elevator = new Elevator { Id = 1, CurrentFloor = 3, CurrentDirection = Direction.Up, MoveSeconds = 10, DwellSeconds = 10 };
        elevator.AddStop(6);

        // passenger at 2: travel up to maxUp (6) => (6-3)*10 =30
        // dwell up stops >3 => one stop (6) => +10
        // travel down to passenger => (6-2)*10 =40
        // dwell at pickup => +10
        ElevatorProcessor.EstimatePickupSeconds(elevator, 2)
            .Should()
            .Be(30 + 10 + 40 + 10);
    }

    [Fact]
    public void EstimatePickupSeconds_Down_WhenPassengerAhead_AddsDwellForStopsBeforePickup()
    {
        var elevator = new Elevator { Id = 1, CurrentFloor = 8, CurrentDirection = Direction.Down, MoveSeconds = 10, DwellSeconds = 10 };
        elevator.AddStop(6);
        elevator.AddStop(2);

        // passenger at 5 -> travel (8-5)=3 floors =>30 + dwell for stop at 6 (10) + dwell at pickup (10)
        ElevatorProcessor.EstimatePickupSeconds(elevator, 5)
            .Should()
            .Be(30 + 10 + 10);
    }

    [Fact]
    public void EstimatePickupSeconds_Down_When_Passenger_Above_Requires_Down_Then_Up()
    {
        var elevator = new Elevator { Id = 1, CurrentFloor = 5, CurrentDirection = Direction.Down, MoveSeconds = 10, DwellSeconds = 10 };
        elevator.AddStop(2); // minDown=2

        // passenger at 7:
        // travel down to minDown: (5-2)=3 *10 =30
        // dwell down stops <5 => one (2) => +10
        // travel up from minDown to passenger: (7-2)=5 *10=50
        // dwell at pickup =10
        ElevatorProcessor.EstimatePickupSeconds(elevator, 7)
            .Should()
            .Be(30 + 10 + 50 + 10);
    }

    [Fact]
    public void SelectBest_Considers_Total_Stops_As_TieBreaker()
    {
        var elevator1 = new Elevator { Id = 1, CurrentFloor = 0 };
        var elevator2 = new Elevator { Id = 2, CurrentFloor = 0 };
        elevator2.AddStop(5); // extra load should make elevator1 preferred

        var best = ElevatorProcessor.SelectBest([elevator1, elevator2], 3);

        best
            .Should()
            .Be(elevator1);
    }

    [Fact]
    public void Enqueue_Does_Not_Duplicate_Stops()
    {
        var elevator = new Elevator { Id = 1, CurrentFloor = 0 };
        elevator.AddStop(4); // preexisting passenger floor

        ElevatorProcessor.Enqueue(elevator, 4, 7);

        elevator
            .GetUpStops()
            .Should()
            .Equal(4, 7);
    }
}
