using System.Collections.Generic;

public class ShipmentPlan
{
    private readonly List<ShipmentEntry> entries = new();

    public int Day { get; }
    public int Quota { get; }
    public float Difficulty { get; }
    public int TotalValue { get; private set; }

    public IReadOnlyList<ShipmentEntry> Entries => entries;
    public bool IsWinnable => TotalValue >= Quota;
    public float Surplus => (float)TotalValue / Quota;

    public ShipmentPlan(int day, int quota, float difficulty)
    {
        Day = day;
        Quota = quota;
        Difficulty = difficulty;
    }

    public void Add(ShipmentEntry entry)
    {
        entries.Add(entry);
        TotalValue += entry.Value;
    }

    public void Replace(int index, ShipmentEntry newEntry)
    {
        TotalValue += newEntry.Value - entries[index].Value;
        entries[index] = newEntry;
    }
}
