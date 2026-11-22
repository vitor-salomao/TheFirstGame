using System.Collections.Generic;

public class ItemPicker
{
    private readonly System.Random random;

    public ItemPicker(System.Random random)
    {
        this.random = random;
    }

    // Items with a higher SpawnWeight are more likely to be picked
    public ItemData Pick(List<ItemData> items)
    {
        float roll = (float)random.NextDouble() * GetTotalWeight(items);

        foreach (ItemData item in items)
        {
            roll -= item.SpawnWeight;
            if (roll < 0f) return item;
        }

        return items[items.Count - 1];
    }

    private float GetTotalWeight(List<ItemData> items)
    {
        float total = 0f;
        foreach (ItemData item in items) total += item.SpawnWeight;
        return total;
    }
}
