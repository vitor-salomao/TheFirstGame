public class ShipmentEntry
{
    public ItemData Item { get; }
    public int Value { get; }
    public ItemSize SlotSize { get; }

    public ShipmentEntry(ItemData item, int value, ItemSize slotSize)
    {
        Item = item;
        Value = value;
        SlotSize = slotSize;
    }
}
