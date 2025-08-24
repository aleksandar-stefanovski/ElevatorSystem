using ElevatorSystem.Worker.Domain.Models;
using ElevatorSystem.Worker.Domain.Requests;

namespace ElevatorSystem.Worker.Services.Interfaces;

public interface IElevatorService
{
    Elevator AssignElevator(ElevatorRequest request);
}