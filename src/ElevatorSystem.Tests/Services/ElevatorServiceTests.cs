using ElevatorSystem.Worker.Domain.Enums;
using ElevatorSystem.Worker.Domain.Interfaces;
using ElevatorSystem.Worker.Domain.Models;
using ElevatorSystem.Worker.Domain.Requests;
using ElevatorSystem.Worker.Services;
using FluentAssertions;
using Moq;

namespace ElevatorSystem.Tests.Services
{
    public class ElevatorServiceTests
    {
        private readonly Mock<IElevatorRepository> _repoMock;
        private readonly ElevatorService _svc;

        public ElevatorServiceTests()
        {
            _repoMock = new Mock<IElevatorRepository>(MockBehavior.Strict);

            _repoMock.Setup(r => r.GetElevators()).Returns([]);

            _svc = new ElevatorService(_repoMock.Object);
        }

        private void SetElevators(params Elevator[] elevators)
        {
            _repoMock.Reset();
            _repoMock.Setup(r => r.GetElevators()).Returns(elevators).Verifiable();
        }

        [Fact]
        public void AssignElevator_Selects_Elevator_With_Lower_Pickup_Estimate_Not_Just_Distance()
        {
            // e1 is *closer in distance* to passenger(2) but is moving UP (away) with queued stop at 6.
            var e1 = new Elevator { Id = 1, CurrentFloor = 4, CurrentDirection = Direction.Up };
            e1.AddStop(6);

            // e2 is farther but IDLE, so overall ETA to pickup should be better.
            var e2 = new Elevator { Id = 2, CurrentFloor = 0, CurrentDirection = Direction.Idle };

            SetElevators(e1, e2);

            var assigned = _svc.AssignElevator(new ElevatorRequest(2, 7));

            assigned
                .Should()
                .BeSameAs(e2);

            _repoMock.Verify(r => r.GetElevators(), Times.Once);

            _repoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void AssignElevator_Queues_Passenger_And_Destination_Upward()
        {
            var e1 = new Elevator { Id = 1, CurrentFloor = 0, CurrentDirection = Direction.Idle };
            SetElevators(e1);

            var assigned = _svc.AssignElevator(new ElevatorRequest(3, 8));

            assigned
                .Should()
                .BeSameAs(e1);

            assigned
                .GetUpStops()
                .Should()
                .Equal(3, 8);

            assigned
                .GetDownStops()
                .Should()
                .BeEmpty();

            assigned.CurrentDirection
                .Should()
                .Be(Direction.Up);

            _repoMock.Verify(r => r.GetElevators(), Times.Once);

            _repoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void AssignElevator_Queues_Passenger_And_Destination_Downward()
        {
            var e1 = new Elevator { Id = 1, CurrentFloor = 9, CurrentDirection = Direction.Idle };
            SetElevators(e1);

            var assigned = _svc.AssignElevator(new ElevatorRequest(6, 2));

            assigned
                .Should()
                .BeSameAs(e1);

            assigned
                .GetDownStops()
                .Should()
                .Equal(6, 2); // sorted desc by design

            assigned
                .GetUpStops()
                .Should()
                .BeEmpty();

            assigned.CurrentDirection
                .Should()
                .Be(Direction.Down);

            _repoMock.Verify(r => r.GetElevators(), Times.Once);
            _repoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void AssignElevator_When_At_Passenger_Floor_Uses_Destination_To_Set_Direction()
        {
            var e1 = new Elevator { Id = 1, CurrentFloor = 5, CurrentDirection = Direction.Idle };
            SetElevators(e1);

            var assigned = _svc.AssignElevator(new ElevatorRequest(5, 2));

            assigned
                .Should()
                .BeSameAs(e1);

            assigned.CurrentDirection
                .Should()
                .Be(Direction.Down);

            assigned
                .GetUpStops()
                .Should()
                .Contain(5); // equality goes to Up list by design

            assigned
                .GetDownStops()
                .Should()
                .Contain(2);

            _repoMock.Verify(r => r.GetElevators(), Times.Once);
            _repoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void AssignElevator_DoesNot_Override_Direction_When_Already_Moving()
        {
            var e1 = new Elevator { Id = 1, CurrentFloor = 5, CurrentDirection = Direction.Up };
            SetElevators(e1);

            _svc.AssignElevator(new ElevatorRequest(2, 1));

            e1.CurrentDirection
                .Should()
                .Be(Direction.Up);

            e1.HasStops
                .Should()
                .BeTrue();

            _repoMock.Verify(r => r.GetElevators(), Times.Once);

            _repoMock.VerifyNoOtherCalls();
        }
    }

    // Keep the ctor guard test close by for clarity
    public class ElevatorRequestTests
    {
        [Fact]
        public void Ctor_Throws_When_Pickup_Equals_Destination()
        {
            Action act = static () => new ElevatorRequest(3, 3);
            act
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("*Pickup and destination floors cannot be the same*");
        }
    }
}
