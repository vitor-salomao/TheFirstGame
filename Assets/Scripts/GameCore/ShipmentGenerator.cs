using System.Collections.Generic;
using UnityEngine;

public class ShipmentGenerator
{
    private readonly List<ItemData> catalog;
    private readonly DifficultyCurve curve;

    public ShipmentGenerator(List<ItemData> catalog, DifficultyCurve curve)
    {
        this.catalog = catalog;
        this.curve = curve;
    }

    public ShipmentPlan Generate(int day, List<ItemSize> slotSizes, System.Random random)
    {
        float difficulty = (float)random.NextDouble();
        int quota = curve.QuotaFor(day, difficulty);
        int maxItemValue = curve.MaxItemValueFor(quota);

        ShipmentPlan plan = new(day, quota, difficulty);
        List<ItemData> pool = FindSpawnableItems(day, maxItemValue);

        Fill(plan, pool, new FreeSlots(slotSizes), random);
        new ShipmentUpgrader(pool, maxItemValue).Upgrade(plan);

        return plan;
    }

    private List<ItemData> FindSpawnableItems(int day, int maxItemValue)
    {
        List<ItemData> pool = new();

        foreach (ItemData item in catalog)
        {
            if (item != null && item.CanSpawn(day, maxItemValue)) pool.Add(item);
        }

        return pool;
    }

    private void Fill(ShipmentPlan plan, List<ItemData> pool, FreeSlots freeSlots, System.Random random)
    {
        int targetValue = curve.TargetValueFor(plan.Quota, plan.Difficulty);
        int maxItemValue = curve.MaxItemValueFor(plan.Quota);
        ItemPicker picker = new(random);

        while (plan.TotalValue < targetValue && plan.Entries.Count < curve.MaxItemsPerDay)
        {
            List<ItemData> itemsThatFit = FindItemsThatFit(pool, freeSlots);
            if (itemsThatFit.Count == 0) return;

            ItemData item = picker.Pick(itemsThatFit);
            ItemSize slotSize = freeSlots.Reserve(item.Size);
            int value = Mathf.Min(item.RollValue(random), maxItemValue);

            plan.Add(new ShipmentEntry(item, value, slotSize));
        }
    }

    private List<ItemData> FindItemsThatFit(List<ItemData> pool, FreeSlots freeSlots)
    {
        List<ItemData> itemsThatFit = new();

        foreach (ItemData item in pool)
        {
            if (freeSlots.HasRoomFor(item.Size)) itemsThatFit.Add(item);
        }

        return itemsThatFit;
    }
}
