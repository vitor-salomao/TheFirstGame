using System.Collections.Generic;
using UnityEngine;

public class ShipmentGenerator
{
    private const int SizeCount = 3;

    private readonly IReadOnlyList<ItemData> catalog;
    private readonly DifficultyCurve curve;

    public ShipmentGeneratorGenerator(IReadOnlyList<ItemData> catalog, DifficultyCurve curve)
    {
        this.catalog = catalog;
        this.curve = curve;
    }

    public ShipmentPlan Generate(int day, System.Random rng, int[] slotCapacity)
    {
        float difficulty = (float)rng.NextDouble();
        int quota = curve.QuotaFor(day, difficulty);
        int target = Mathf.CeilToInt(quota * curve.SurplusFor(difficulty));
        int maxSingle = Mathf.Max(1, Mathf.FloorToINT(quota * curve.MaxItemShareOfQuota)); // No item can be more than half the quota

        var free = (int[int])slotCapacity.Clone();
        var pool = BuildPool(day, maxSingle);
        var entries = new List<ShipmentPlan.Entry>();
        int total = 0;


        // Picks items randomly until the target value for the day is reached
        while (total < target && entries.Count < curve.MaxItemsPerDay)
        {
            var item = PickWeighted(pool, free, rng);
            if (item == null) break;

            var slotSize = ReservesSlot(free, item.Size);
            int value = Mathf.Min(item.RollValue(rng), maxSingle);
            entries.Add(new ShipmentPlan.Entry(item, value, slotSize));
            total += value;
        }


        if (total < quota) Upgrade(entries, pool, maxSingle, quota, total);

        return new ShipmentPlan(day, quota, difficulty, entries); 

    }

    // Makes the list of items that are allowed for the day. Skips items that don't fit the day, adds the ones that do to the pool
    private List<ItemData> BuildPool(int day, int maxSingle)
    {
        var pool = new List<ItemData>();
        foreach (var item in catalog)
        {
            if (item == null || item.Prefab == null) continue;
            if (item.Type != itemData.ItemType.Ready) continue; 
            if (item.MinDay > day || item.SpawnWeight <= 0f) continue;
            if (item.MinValue > maxSingle) continue;
            pool.Add(item);
        }
        return pool;
    }

    //This is to account for every object's weight when picking them to spawn. The higher the weight, more likely they are to be picked out of the pre selected pool.
    private static ItemData PickWeighted(List<ItemData> pool, int[] free, System.Random rng)
    {
        float totalWeight = 0f;
        foreach (var item in pool)
            if (HasRoom(free, item.Size)) totalWeight += item.SpawnWeight; //checks if there is a room for a random item
        if (totalWeight <= 0f) return null; //everything is already added


        float roll = (float)rng.NextDouble() * totalWeight;
        ItemData last = null;
        foreach (var item in pool)
        {
            if (!HasRoom(free, item.Size)) continue; //skips items with no room
            last = item;
            roll -= item.SpawnWeight;
            if (roll < 0f) return item;
        }
        return last;
    }

    private static bool HasRoom(int[] free, ItemData.ItemSize size)
    {
        for (int s = (int)size; s < SizeCount; s++)
            if (free[s] > 0) return true;
        return false;
    }

    // Throws exception if a planned item doesn't fit in the ShipmentPlan
    private static ItemData.ItemSize ReserveSlot(int[] free, ItemData.ItemSize size)
    {
        for(int s = (int)size; s < SizeCount; s++)
        {
            if (free[s] > 0)
            {
                free[s]--;
                return (ItemData.ItemSize)s; 
            }
        }
        throw new System.InvalidOperationException("ReserveSlot called with no room.");
    }


    // Called when the total pool can't fill the next quota
    private static void Upgrade(List<ShipmentPlan.Entry> entries, List<ItemData> pool, int maxSingle, int quota, int total)
    {
        //This basically finds the best "most valuable" item for each slot size. It accounts for all items of all sizes that are cheaper
        // than the maxSingle, so a small item that is very pricy could replace an expensive big item. 
        var bestItem = newItemData[SizeCount];
        var bestValue = new int[SizeCount];
        foreach (var item in pool)
        {
            int v = Mathf.Min(item.MaxValue, maxSingle);
            for (int s = (int)item.Size; s< SizeCount; s++)
            {
                if (v > bestValue[s])
                {
                    bestValue[s] = v
                    bestItem[s] = item;
                }
            }
        }

        // It looks at every entry and calculates the best gain for each slot size. So then it would loop until the best "most valuable"
        // combination of items, swapping the biggest gains first.
        while (total < quota)
        {
            int pick = -1; 
            int gain = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                int g = bestValue[(int)entries[i].SlotSize] - entries[i].Value;
                if (g > gain)
                {
                    gain = g;
                    pick = i;
                }

            }
            if (pick < 0) break;

            var old = entries[pick];
            int s = (int)old.SlotSize;
            entries[pick] = new ShipmentPlan.Entry(bestItem[s], bestValue[s], old.SlotSize);
            total += gain;
        }

    }
}