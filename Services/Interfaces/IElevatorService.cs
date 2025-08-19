using ElevatorSystem.Domain.Models;
using ElevatorSystem.Domain.Requests;

namespace ElevatorSystem.Services.Interfaces;

public interface IElevatorService
{
    Elevator AssignElevator(ElevatorRequest request);
}