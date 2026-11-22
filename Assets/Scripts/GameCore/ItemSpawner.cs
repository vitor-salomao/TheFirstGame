using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    private const float SurfaceGap = 0.01f;

    [Tooltip("Parent of all the SpawnSlots on the map.")]
    [SerializeField] private Transform slotRoot;
    [Tooltip("Spawned items go under this object.")]
    [SerializeField] private Transform itemContainer;

    private readonly List<WorldItem> spawned = new();

    public IReadOnlyList<WorldItem> Spawned => spawned;

    public List<ItemSize> GetSlotSizes()
    {
        List<ItemSize> sizes = new();
        foreach (SpawnSlot slot in GetSlots()) sizes.Add(slot.Size);
        return sizes;
    }

    public void Spawn(ShipmentPlan plan, System.Random random)
    {
        Clear();

        List<SpawnSlot> freeSlots = GetSlots();
        Shuffle(freeSlots, random);

        foreach (ShipmentEntry entry in plan.Entries)
        {
            SpawnSlot slot = TakeSlot(freeSlots, entry.SlotSize);

            if (slot != null) SpawnItem(entry, slot);
            else Debug.LogError($"No free {entry.SlotSize} slot for {entry.Item.Name}", this);
        }
    }

    public void Clear()
    {
        foreach (WorldItem item in spawned)
        {
            if (item != null) Destroy(item.gameObject);
        }

        spawned.Clear();
    }

    private void SpawnItem(ShipmentEntry entry, SpawnSlot slot)
    {
        Transform point = slot.transform;
        WorldItem item = Instantiate(entry.Item.Prefab, point.position, point.rotation, itemContainer);

        item.Init(entry.Item, entry.Value);
        RestOnSurface(item, point.position.y);
        spawned.Add(item);
    }

    // Lifts the item so its lowest point sits on the surface
    private void RestOnSurface(WorldItem item, float surfaceHeight)
    {
        Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        float lowestPoint = renderers[0].bounds.min.y;
        foreach (Renderer part in renderers) lowestPoint = Mathf.Min(lowestPoint, part.bounds.min.y);

        item.transform.position += Vector3.up * (surfaceHeight - lowestPoint + SurfaceGap);
    }

    private List<SpawnSlot> GetSlots()
    {
        return new List<SpawnSlot>(slotRoot.GetComponentsInChildren<SpawnSlot>());
    }

    private SpawnSlot TakeSlot(List<SpawnSlot> slots, ItemSize size)
    {
        foreach (SpawnSlot slot in slots)
        {
            if (slot.Size == size)
            {
                slots.Remove(slot);
                return slot;
            }
        }

        return null;
    }

    private void Shuffle(List<SpawnSlot> slots, System.Random random)
    {
        for (int i = slots.Count - 1; i > 0; i--)
        {
            int other = random.Next(i + 1);
            SpawnSlot temp = slots[i];
            slots[i] = slots[other];
            slots[other] = temp;
        }
    }
}
