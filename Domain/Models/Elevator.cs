using ElevatorSystem.Domain.Enums;

namespace ElevatorSystem.Domain.Models;

public class Elevator
{
    private readonly List<int> _upStops = [];

    private readonly List<int> _downStops = [];

    public required int Id { get; init; }

    public int DwellSeconds { get; set; } = 10;

    public int MoveSeconds { get; set; } = 10;

    public int CurrentFloor { get; set; } = 0;

    public bool HasStops => _upStops.Count > 0 || _downStops.Count > 0;

    public Direction CurrentDirection { get; set; } = Direction.Idle;

    public int UpStopsCount() => _upStops.Count;

    public int DownStopsCount() => _downStops.Count;

    public IReadOnlyList<int> GetUpStops() => _upStops.AsReadOnly();

    public IReadOnlyList<int> GetDownStops() => _downStops.AsReadOnly();

    public void AddStop(int floor)
    {
        if (floor > CurrentFloor)
        {
            if (!_upStops.Contains(floor))
            {
                _upStops.Add(floor);
                _upStops.Sort();
            }
        }
        else if (floor < CurrentFloor)
        {
            if (!_downStops.Contains(floor))
            {
                _downStops.Add(floor);
                _downStops.Sort();
                _downStops.Reverse();
            }
        }
        else
        {
            if (!_upStops.Contains(floor))
            {
                _upStops.Add(floor);
                _upStops.Sort();
            }
        }
    }

    public void RemoveStopAtCurrent()
    {
        _upStops.RemoveAll(f => f == CurrentFloor);
        _downStops.RemoveAll(f => f == CurrentFloor);

        if (!HasStops)
            CurrentDirection = Direction.Idle;

        else CurrentDirection = _upStops.Count > 0
                ? Direction.Up
                : Direction.Down;
    }
}
