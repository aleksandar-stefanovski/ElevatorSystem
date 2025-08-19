namespace ElevatorSystem.Configuration;

public class ElevatorConfiguration
{
    public required int MinFloor { get; set; } = 0;

    public required int MaxFloor { get; set; } = 10;

    public required int RequestPeriodSeconds { get; set; } = 5;

    public required int RandomSeed { get; set; } = 40;
}
