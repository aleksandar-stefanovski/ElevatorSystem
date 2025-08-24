using ElevatorSystem.Worker.Domain.Enums;

namespace ElevatorSystem.Worker.Domain.Models;

/// <summary>
/// In-memory mutable model that represents a single elevator car.
/// </summary>
public class Elevator
{
    // Floors queued for travel while moving / planning upward (ascending order).
    private readonly List<int> _upStops = [];

    // Floors queued for travel while moving / planning downward (descending order).
    private readonly List<int> _downStops = [];

    public required int Id { get; init; }

    /// <summary>Seconds doors stay open while servicing a stop.</summary>
    public int DwellSeconds { get; set; } = 10;

    /// <summary>Seconds needed to travel exactly one floor.</summary>
    public int MoveSeconds { get; set; } = 10;

    /// <summary>The current floor (updated by the mover loop).</summary>
    public int CurrentFloor { get; set; } = 0;

    /// <summary>True when any pending stop exists.</summary>
    public bool HasStops => _upStops.Count > 0 || _downStops.Count > 0;

    /// <summary>Current travel direction. Set to Idle when no stops remain.</summary>
    public Direction CurrentDirection { get; set; } = Direction.Idle;

    public int UpStopsCount() => _upStops.Count;

    public int DownStopsCount() => _downStops.Count;

    public IReadOnlyList<int> GetUpStops() => _upStops.AsReadOnly();

    public IReadOnlyList<int> GetDownStops() => _downStops.AsReadOnly();

    /// <summary>
    /// Enqueue a floor respecting ordering + de‑duplication rules.
    /// </summary>
    public void AddStop(int floor)
    {
        if (floor > CurrentFloor)
        {
            // Upward stop beyond current position.
            if (!_upStops.Contains(floor))
            {
                _upStops.Add(floor);
                _upStops.Sort(); // keep ascending
            }
        }
        else if (floor < CurrentFloor)
        {
            // Downward stop below current position.
            if (!_downStops.Contains(floor))
            {
                _downStops.Add(floor);
                _downStops.Sort();     // ascending
                _downStops.Reverse();  // turn into descending
            }
        }
        else
        {
            // Equal -> treat as Up (design choice so removal + direction selection is deterministic).
            if (!_upStops.Contains(floor))
            {
                _upStops.Add(floor);
                _upStops.Sort();
            }
        }
    }

    /// <summary>
    /// Remove any stop matching the current floor and decide the next direction.
    /// Preference: if any up stops remain -> Up, else Down, else Idle.
    /// </summary>
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
