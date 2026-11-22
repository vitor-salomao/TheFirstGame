using System;
using System.Collections.Generic;

public class FreeSlots
{
    private readonly List<ItemSize> slotSizes;

    public FreeSlots(List<ItemSize> slotSizes)
    {
        this.slotSizes = new List<ItemSize>(slotSizes);
        this.slotSizes.Sort();
    }

    public bool HasRoomFor(ItemSize itemSize)
    {
        foreach (ItemSize slotSize in slotSizes)
        {
            if (slotSize >= itemSize) return true;
        }

        return false;
    }

    // Slots are sorted from small to large, so the first one that fits is the tightest fit
    public ItemSize Reserve(ItemSize itemSize)
    {
        foreach (ItemSize slotSize in slotSizes)
        {
            if (slotSize >= itemSize)
            {
                slotSizes.Remove(slotSize);
                return slotSize;
            }
        }

        throw new InvalidOperationException($"No free slot for a {itemSize} item.");
    }
}
