namespace ElevatorSystem.Worker.Presentation;

public interface IElevatorDisplay
{
    Task DisplayAsync(CancellationToken token);
}
