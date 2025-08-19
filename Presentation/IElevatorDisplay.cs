namespace ElevatorSystem.Presentation;

public interface IElevatorDisplay
{
    Task DisplayAsync(CancellationToken token);
}
