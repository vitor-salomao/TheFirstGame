using System.Collections.Generic;

// What spawns today and what it's worth. Pure data, produced by ShipmentGenerator.
public class ShipmentPlan
{
    //Each entry is one item.
    public readonly struct Entry
    {
        public readonly ItemData Item;
        public readonly int Value;
        /// The slot size reserved for this item (can be larger than the item itself).
        public readonly ItemData.ItemSize SlotSize;

        public Entry(ItemData item, int value, ItemData.ItemSize slotSize)
        {
            Item = item;
            Value = value;
            SlotSize = slotSize;
        }
    }

    public int Day { get; }
    public int Quota { get; }
    public float Difficulty { get; }
    public IReadOnlyList<Entry> Entries { get; }
    public int TotalValue { get; }

    public bool IsWinnable => TotalValue >= Quota;
    public float Surplus => Quota > 0 ? (float)TotalValue / Quota : 0f;


    public ShipmentPlan(int day, int quota, float difficulty, List<Entry> entries)
    {
        Day = day;
        Quota = quota;
        Difficulty = difficulty;
        Entries = entries;

        int total = 0;
        foreach (var e in entries) total += e.Value;
        TotalValue = total;
    }
}