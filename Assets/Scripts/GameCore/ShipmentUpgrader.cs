using System.Collections.Generic;
using UnityEngine;

// Swaps cheap items for valuable ones until the day can be won
public class ShipmentUpgrader
{
    private readonly List<ItemData> pool;
    private readonly int maxItemValue;

    public ShipmentUpgrader(List<ItemData> pool, int maxItemValue)
    {
        this.pool = pool;
        this.maxItemValue = maxItemValue;
    }

    public void Upgrade(ShipmentPlan plan)
    {
        while (!plan.IsWinnable)
        {
            int index = FindBestSwap(plan);
            if (index == -1) return;

            plan.Replace(index, GetBestEntry(plan.Entries[index].SlotSize));
        }
    }

    // Returns -1 when no swap would increase the value
    private int FindBestSwap(ShipmentPlan plan)
    {
        int bestIndex = -1;
        int bestGain = 0;

        for (int i = 0; i < plan.Entries.Count; i++)
        {
            ShipmentEntry current = plan.Entries[i];
            int gain = GetBestEntry(current.SlotSize).Value - current.Value;

            if (gain > bestGain)
            {
                bestGain = gain;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private ShipmentEntry GetBestEntry(ItemSize slotSize)
    {
        ItemData bestItem = null;
        int bestValue = 0;

        foreach (ItemData item in pool)
        {
            if (item.Size > slotSize) continue;

            int value = Mathf.Min(item.MaxValue, maxItemValue);
            if (value > bestValue)
            {
                bestValue = value;
                bestItem = item;
            }
        }

        return new ShipmentEntry(bestItem, bestValue, slotSize);
    }
}
