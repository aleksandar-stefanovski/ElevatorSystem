using ElevatorSystem.Worker.Domain.Enums;
using ElevatorSystem.Worker.Domain.Models;
using FluentAssertions;

namespace ElevatorSystem.Tests.Domain
{
    public class ElevatorModelTests
    {
        [Fact]
        public void AddStop_SortsAndRemovesDuplicates_UpAsc_DownDesc()
        {
            var elevator = new Elevator { Id = 1, CurrentFloor = 5 };

            elevator.AddStop(7);
            elevator.AddStop(9);
            elevator.AddStop(7);
            elevator.AddStop(3);
            elevator.AddStop(5);

            elevator
                .GetUpStops()
                .Should()
                .Equal(5, 7, 9);

            elevator
                .GetDownStops()
                .Should()
                .Equal(3);
        }

        [Fact]
        public void AddStop_EqualToCurrent_GoesToUpStops()
        {
            // Demonstrates the rule: a stop equal to current floor is treated as an "up" stop.
            var elevator = new Elevator { Id = 2, CurrentFloor = 4 };

            elevator.AddStop(4);
            elevator.AddStop(4); // duplicate ignored

            elevator
                .GetUpStops()
                .Should()
                .Equal(4);

            elevator
                .GetDownStops()
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void AddStop_DownStops_SortedDesc_RemovesDuplicates()
        {
            // Down list must remain strictly descending with duplicates removed.
            var elevator = new Elevator { Id = 1, CurrentFloor = 8 };

            elevator.AddStop(3);
            elevator.AddStop(6); // still below current so down
            elevator.AddStop(1);
            elevator.AddStop(3); // duplicate ignored

            elevator
                .GetDownStops()
                .Should()
                .Equal(6, 3, 1);
        }

        [Fact]
        public void RemoveStopAtCurrent_RemovesStop_And_SetsIdle_When_NoMoreStops()
        {
            // When we service the only stop left, direction should fall back to Idle.
            var elevator = new Elevator { Id = 3, CurrentFloor = 3, CurrentDirection = Direction.Up };
            elevator.AddStop(3); // goes to up list

            elevator.RemoveStopAtCurrent();

            elevator.HasStops
                .Should()
                .BeFalse();

            elevator
                .GetUpStops()
                .Should()
                .BeEmpty();

            elevator
                .GetDownStops()
                .Should()
                .BeEmpty();

            elevator.CurrentDirection
                .Should()
                .Be(Direction.Idle);
        }

        [Fact]
        public void RemoveStopAtCurrent_When_UpRemains_SetsDirectionUp()
        {
            // If only up stops remain after removing current, direction flips to Up.
            var elevator = new Elevator { Id = 4, CurrentFloor = 5, CurrentDirection = Direction.Down };

            elevator.AddStop(5); // current floor stop (up list by design)
            elevator.AddStop(8); // another up

            elevator.RemoveStopAtCurrent(); // removes 5

            elevator
                .GetUpStops()
                .Should()
                .Equal(8);

            elevator
                .GetDownStops()
                .Should()
                .BeEmpty();

            elevator.CurrentDirection
                .Should()
                .Be(Direction.Up);
        }

        [Fact]
        public void RemoveStopAtCurrent_When_DownRemains_SetsDirectionDown()
        {
            // If only down stops remain after servicing current floor, direction becomes Down.
            var elevator = new Elevator { Id = 5, CurrentFloor = 2, CurrentDirection = Direction.Up };

            // Temporarily move up so the stops register as below current.
            elevator.CurrentFloor = 3;
            elevator.AddStop(2);
            elevator.AddStop(0);
            elevator.CurrentFloor = 2; // now at one of the down stops

            elevator
                .GetDownStops()
                .Should()
                .Contain(2);

            elevator.RemoveStopAtCurrent();

            elevator
                .GetDownStops()
                .Should()
                .Equal(0);

            elevator
                .GetUpStops()
                .Should()
                .BeEmpty();

            elevator
                .CurrentDirection
                .Should()
                .Be(Direction.Down);
        }

        [Fact]
        public void RemoveStopAtCurrent_When_Up_And_Down_Remain_Prefers_Up()
        {
            // Preference rule: if both lists still have stops, elevator chooses Up.
            var elevator = new Elevator { Id = 6, CurrentFloor = 5, CurrentDirection = Direction.Down };

            // current floor stop (goes to up list), plus other up & down stops
            elevator.AddStop(5);
            elevator.AddStop(7);
            elevator.AddStop(3);
            elevator.AddStop(1);

            elevator.RemoveStopAtCurrent(); // removes 5 only

            elevator
                .GetUpStops()
                .Should()
                .Equal(7);

            elevator
                .GetDownStops()
                .Should()
                .Equal(3, 1);

            elevator.CurrentDirection
                .Should()
                .Be(Direction.Up); // prefers Up when both remain
        }

        [Fact]
        public void HasStops_True_When_Either_List_Not_Empty()
        {
            // Verifies HasStops is true if at least one stop exists (Up or Down).
            var elevator = new Elevator { Id = 7, CurrentFloor = 4 };
            elevator.HasStops
                .Should()
                .BeFalse();

            elevator.AddStop(6);

            elevator.HasStops
                .Should()
                .BeTrue();
        }
    }
}
