namespace ElevatorSystem.Worker.Configuration;

public class ElevatorConfiguration
{
    public required int MinFloor { get; set; } = 0;

    public required int MaxFloor { get; set; } = 9;

    public required int RequestPeriodSeconds { get; set; } = 3500;

    public required int RandomSeed { get; set; } = 40;

    public int IdleCheckDelayMilliseconds { get; set; } = 50;

    public int IdleSleepDelayMilliseconds { get; set; } = 200;
}
